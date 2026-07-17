using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResult>>;

public sealed record LoginResult(string AccessToken);
