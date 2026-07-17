using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Seats.ReleaseSeats;

public sealed class ReleaseSeatsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/events/{eventId:guid}/seats/release", async (
                Guid eventId, ReleaseSeatsRequest request, IMediator mediator) =>
            {
                var command = new ReleaseSeatsCommand(eventId, request.SeatIds);

                var result = await mediator.Send(command);
                return result.ToHttpResult();
            })
            .RequireAuthorization();
    }
}
