using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Seats.LockSeats
{
    public sealed class LockSeatsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/events/{eventId:guid}/seats/lock", async (Guid eventId, LockSeatsRequest request, IMediator mediator) =>
            {
                var command = new LockSeatsCommand(eventId, request.SeatIds);

                var result = await mediator.Send(command);
                return result.ToHttpResult();
            })
            .RequireAuthorization()
            .RequireRateLimiting("SeatLockPolicy");
        }
    }
}
