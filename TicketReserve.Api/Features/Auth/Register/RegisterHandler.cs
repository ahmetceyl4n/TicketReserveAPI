using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;

namespace TicketReserve.Api.Features.Auth.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IPublishEndpoint publishEndpoint)
    {
        _userManager = userManager;
        _db = db;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<RegisterResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Result.Failure<RegisterResult>(AuthErrors.EmailAlreadyExists);
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        // UserManager kendi SaveChanges'ını yapar; kullanıcı yazımını ve outbox mesajını tek commit'e
        // bağlamak için explicit transaction. Publish, SaveChanges'tan önce çağrılır; MassTransit outbox
        // interceptor mesajı aynı transaction'da yazar, Commit ile ikisi birlikte kalıcı olur.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(error => error.Description));
            return Result.Failure<RegisterResult>(new Error("Auth.RegisterFailed", message, ErrorType.Failure));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            // Commit'e ulaşılmadan transaction dispose olacak → CreateAsync'in yazdığı kullanıcı da
            // rollback olur.
            throw new InvalidOperationException(
                $"Kullanıcı oluşturuldu ama 'User' rolü atanamadı: " +
                $"{string.Join(" ", roleResult.Errors.Select(error => error.Description))}");
        }

        var integrationEvent = new UserRegisteredIntegrationEvent(user.Id, user.Email!, user.FullName);
        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result.Success(new RegisterResult(user.Id, user.Email!, user.FullName));
    }
}
