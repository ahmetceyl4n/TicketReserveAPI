using MediatR;
using Microsoft.AspNetCore.Identity;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;
using TicketReserve.Api.Infrastructure.Auth;

namespace TicketReserve.Api.Features.Auth.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(UserManager<ApplicationUser> userManager, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Failure<LoginResult>(AuthErrors.InvalidCredentials);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return Result.Failure<LoginResult>(AuthErrors.InvalidCredentials);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return Result.Success(new LoginResult(token));
    }
}
