using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using TicketReserve.Api.Data;
using TicketReserve.Api.Infrastructure.Auth;
using TicketReserve.Api.Infrastructure.Behaviors;
using TicketReserve.Api.Infrastructure.Correlation;
using TicketReserve.Api.Infrastructure.Endpoints;
using TicketReserve.Api.Infrastructure.ExceptionHandling;
using TicketReserve.Api.Infrastructure.Locking;
using TicketReserve.Api.Infrastructure.Seed;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("TicketReserve.Api başlatılıyor");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    builder.Services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
    builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    builder.Services.AddEndpoints(typeof(Program).Assembly);
    // Kayıt sırası önemli: Logging en dışta sarmalı, Validation onun içinde çalışmalı —
    // böylece loglama, validasyon hatalarını da (handler'a hiç ulaşmasalar bile) görür.
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            var correlationIdProvider = context.HttpContext.RequestServices.GetRequiredService<ICorrelationIdProvider>();
            context.ProblemDetails.Extensions["correlationId"] = correlationIdProvider.CorrelationId;
        };
    });
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.AddInterceptors(new SoftDeleteInterceptor());
    });

    // Singleton: ConnectionMultiplexer kendi TCP bağlantı havuzunu ve thread-safe multiplexing'i
    // zaten yönetiyor. Scoped/Transient yapıp her istekte yeni bir tane açmak, her HTTP isteğinde
    // yeni bir Redis TCP bağlantısı kurmak demek olurdu — StackExchange.Redis'in kendisi bunun
    // yerine tek bağlantı üzerinden binlerce isteği aynı anda multiplexle çalıştırmak için tasarlandı.
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
    builder.Services.AddSingleton<ISeatLockService, RedisSeatLockService>();

    builder.Services
        // AddIdentity yerine AddIdentityCore: AddIdentity, cookie tabanlı bir authentication
        // şeması da kaydedip onu varsayılan (DefaultChallengeScheme) yapıyor — bu da aşağıdaki
        // JWT Bearer ayarımızı geçersiz kılıp [Authorize] reddettiğinde 401 yerine
        // "/Account/Login"a yönlendirme davranışına yol açıyordu. AddIdentityCore, cookie
        // şemasını hiç kaydetmiyor; SignInManager'ı da kullanmadığımız için (UserManager'ın
        // kendi CheckPasswordAsync'i yeterli) eksikliği bizi etkilemiyor.
        .AddIdentityCore<ApplicationUser>(options =>
        {
            // FluentValidation'daki RegisterCommandValidator kurallarıyla bilerek aynı:
            // ikisi çakışırsa kullanıcı validasyonu geçip Identity katmanında beklenmedik şekilde reddedilir.
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("'Jwt' konfigürasyon bölümü eksik.");

    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddRateLimiter(options =>
    {
        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }

            await context.HttpContext.Response.WriteAsJsonAsync(
                new { message = "Çok fazla istek gönderildi. Lütfen bir süre sonra tekrar deneyin." },
                cancellationToken);
        };

        options.AddPolicy("LoginPolicy", httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
        options.AddPolicy("SeatLockPolicy", httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapControllers();

    app.MapEndpoints();

    await IdentitySeeder.SeedAsync(app);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException, "dotnet ef migrations" gibi tasarım zamanı (design-time) araçların
    // DbContext'i incelemek için host'u kasıtlı olarak durdurmasıyla oluşur — gerçek bir hata değil.
    Log.Fatal(ex, "TicketReserve.Api başlatılırken beklenmedik bir hata oluştu");
}
finally
{
    Log.CloseAndFlush();
}
