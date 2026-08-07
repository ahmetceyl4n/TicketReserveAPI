using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Reservations.CancelReservation
{
    public sealed record CancelReservationCommand(Guid ReservationId) : IRequest<Result>;

   
}
