using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data.Entities;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Reservations.CancelReservation
{
    public class CancelReservationEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/reservations/{reservationId:guid}", async (Guid reservationId, ISender mediator) =>
                    (await mediator.Send(new CancelReservationCommand(reservationId))).ToHttpResult())
                .RequireAuthorization();
        }
    }
}
