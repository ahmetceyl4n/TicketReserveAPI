using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using TicketReserve.Api.Data;

namespace TicketReserve.Api.Tests
{
    public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16").Build();

        private readonly RedisContainer _redis = new RedisBuilder()
            .WithImage("redis:7").Build();

        private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management").Build();

        private const string TestJwtKey = "integration-test-signing-key-at-least-32-bytes-long";

        public IntegrationTestFactory()
        {
            // Jwt:Key, Program.cs'te builder.Build()'den ÖNCE okunuyor; WebApplicationFactory'nin
            // ConfigureAppConfiguration override'ı ancak build'den SONRA devreye girdiği için o erken
            // okumaya yetişmiyor. Env var ise CreateBuilder tarafından erken eklenir → okuma onu görür.
            Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                    ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
                    ["ConnectionStrings:RabbitMQ"] = _rabbitMq.GetConnectionString(),
                    // Test host user-secrets yüklemez → normalde secret olan bu değerleri elle veriyoruz.
                    ["Jwt:Key"] = TestJwtKey,
                    ["SeedAdmin:Email"] = "admin@test.local",
                    ["SeedAdmin:Password"] = "Admin123!",
                });
            });

            // JWT DOĞRULAMASI, uygulama açılışında (builder.Build()'den ÖNCE) Jwt:Key'i okuyor; test
            // override'ımız build sırasında uygulandığı için doğrulama farklı bir anahtar kullanıyordu → 401.
            // Doğrulama anahtarını burada (build'den sonra) test anahtarına sabitliyoruz ki üretimle aynı olsun.
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
                });
            });
        }
        async Task IAsyncLifetime.InitializeAsync()
        {
            await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync(), _rabbitMq.StartAsync());
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            await using var db = new ApplicationDbContext(options);
            await db.Database.MigrateAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _postgres.DisposeAsync();
            await _redis.DisposeAsync();
            await _rabbitMq.DisposeAsync();
            await base.DisposeAsync();
        }

        
    }
}
