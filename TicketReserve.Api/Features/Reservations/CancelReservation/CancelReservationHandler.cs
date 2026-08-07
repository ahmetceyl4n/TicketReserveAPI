using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;
using TicketReserve.Api.Infrastructure.Auth;
using TicketReserve.Api.Infrastructure.Realtime;

namespace TicketReserve.Api.Features.Reservations.CancelReservation
{
    public class CancelReservationHandler : IRequestHandler<CancelReservationCommand, Result>
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICurrentUserService _currentUser;
        private readonly IPublisher _publisher;

        public CancelReservationHandler(ApplicationDbContext dbContext, ICurrentUserService currentUser, IPublisher publisher)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _publisher = publisher;
        }

        public async Task<Result> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _dbContext.Reservations
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(r=> r.Id == request.ReservationId,cancellationToken);

            if (reservation is null)
                return Result.Failure(ReservationErrors.NotFound);

            if (reservation.UserId != _currentUser.UserId)
                return Result.Failure(ReservationErrors.NotOwned);

            reservation.Seat.Status = SeatStatus.Available;
            _dbContext.Reservations.Remove(reservation);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publisher.Publish(
                new SeatStatusChangedNotification(reservation.Seat.EventId, reservation.SeatId, "Available"), cancellationToken);

            return Result.Success();

        }
    }
}
