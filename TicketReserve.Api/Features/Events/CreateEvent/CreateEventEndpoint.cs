using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Events.CreateEvent;

public sealed class CreateEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/events", async (CreateEventCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.ToHttpResult();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
