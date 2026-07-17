using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Seats.PurchaseSeats;

public sealed class PurchaseSeatsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/events/{eventId:guid}/seats/purchase", async (
                Guid eventId, PurchaseSeatsRequest request, IMediator mediator) =>
            {
                var command = new PurchaseSeatsCommand(eventId, request.SeatIds);

                var result = await mediator.Send(command);
                return result.ToHttpResult();
            })
            .RequireAuthorization();
    }
}
