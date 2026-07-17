using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Auth.Register;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/register", async (RegisterCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.ToHttpResult();
        }).RequireRateLimiting("LoginPolicy");
    }
}
