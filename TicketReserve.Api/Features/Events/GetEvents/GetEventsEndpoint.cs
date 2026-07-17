using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Events.GetEvents;

public sealed class GetEventsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/events", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetEventsQuery());
            return result.ToHttpResult();
        });
    }
}
