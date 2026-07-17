using MediatR;
using Microsoft.AspNetCore.Identity;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;

namespace TicketReserve.Api.Features.Auth.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var message = string.Join(" ", createResult.Errors.Select(error => error.Description));
            return Result.Failure<RegisterResult>(new Error("Auth.RegisterFailed", message, ErrorType.Failure));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Kullanıcı oluşturuldu ama 'User' rolü atanamadı: " +
                $"{string.Join(" ", roleResult.Errors.Select(error => error.Description))}");
        }

        return Result.Success(new RegisterResult(user.Id, user.Email!, user.FullName));
    }
}
