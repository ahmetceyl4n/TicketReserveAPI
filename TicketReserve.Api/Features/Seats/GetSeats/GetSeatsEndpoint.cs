using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Seats.GetSeats;

public sealed class GetSeatsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/events/{eventId:guid}/seats", async (Guid eventId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSeatsQuery(eventId));
            return result.ToHttpResult();
        });
    }
}
