using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Events.DeleteEvent;

public sealed class DeleteEventEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/events/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteEventCommand(id));
                return result.ToHttpResult();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
