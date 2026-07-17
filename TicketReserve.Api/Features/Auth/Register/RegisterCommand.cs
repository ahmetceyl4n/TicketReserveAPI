using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Auth.Register;

public sealed record RegisterCommand(string Email, string Password, string FullName)
    : IRequest<Result<RegisterResult>>;

public sealed record RegisterResult(string Id, string Email, string FullName);
