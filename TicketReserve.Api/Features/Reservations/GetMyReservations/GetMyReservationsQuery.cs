using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Reservations.GetMyReservations
{
    public class GetMyReservationsQuery : IRequest<Result<List<ReservationSummary>>>
    {
    }
}
