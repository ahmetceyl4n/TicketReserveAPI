using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Infrastructure.Auth;

namespace TicketReserve.Api.Features.Reservations.GetMyReservations
{
    public class GetMyReservationsHandler : IRequestHandler<GetMyReservationsQuery, Result<List<ReservationSummary>>>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        public GetMyReservationsHandler(ApplicationDbContext context, ICurrentUserService currentUser)
        {
            _dbContext = context;
            _currentUser = currentUser;
        }

        

        public async Task<Result<List<ReservationSummary>>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
        {
            var reservationList = await _dbContext.Reservations
                .Where(r => r.UserId == _currentUser.UserId)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAtUtc)
                .Select(r => new ReservationSummary(
                    r.Id,
                    r.Seat.EventId,
                    r.Seat.Event.Name,      
                    r.Seat.Event.Venue,
                    r.Seat.Event.Date,
                    r.Seat.Row,
                    r.Seat.Number,
                    r.PriceAtPurchase,
                    r.CreatedAtUtc))
                .ToListAsync(cancellationToken);

            return Result.Success(reservationList);
        }
    }
}
