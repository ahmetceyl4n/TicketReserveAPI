using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Infrastructure.Auth;
using TicketReserve.Api.Infrastructure.Locking;

namespace TicketReserve.Api.Features.Seats.PurchaseSeats
{
    public sealed class PurchaseSeatsHandler : IRequestHandler<PurchaseSeatsCommand, Result>
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly ISeatLockService _seatLockService;
        private readonly ICurrentUserService _currentUser;
        private readonly IPublisher _publisher;

        public PurchaseSeatsHandler(ApplicationDbContext dbcontext, ISeatLockService seatLockService, ICurrentUserService currentUser, IPublisher publisher)
        {
            _dbcontext = dbcontext;
            _seatLockService = seatLockService;
            _currentUser = currentUser;
            _publisher = publisher;
        }

        public async Task<Result> Handle(PurchaseSeatsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var isLockOwner = await _seatLockService.AreAllLockedByAsync(request.SeatIds, userId);

            if (!isLockOwner)
            {
                return Result.Failure(SeatErrors.LockNotOwned);
            }

            var seats = await _dbcontext.Seats
                .Where(s => s.EventId == request.EventId && request.SeatIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            foreach (var seat in seats)
            {
                if (seat.Status != SeatStatus.Available)
                {
                    return Result.Failure(SeatErrors.AlreadyBooked);
                }
            }

            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.Sold;
                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    SeatId = seat.Id,
                    UserId = userId,
                    PriceAtPurchase = seat.Price,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _dbcontext.Reservations.Add(reservation);
            }

            try
            {
                await _dbcontext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Aynı SaveChangesAsync çağrısında iki bağımsız savunma katmanı var: Seat.xmin
                // (UPDATE'te 0 satır etkilenirse DbUpdateConcurrencyException) ve Reservation.SeatId
                // üzerindeki unique index (INSERT çakışırsa Postgres unique_violation, EF Core'da
                // düz DbUpdateException). Hangisinin önce tetikleneceği bir garanti değil — ikisi de
                // aynı şeyi ifade ediyor: biri bizden önce satın aldı. DbUpdateConcurrencyException zaten
                // DbUpdateException'ın alt sınıfı, o yüzden tek catch ikisini de yakalıyor.
                return Result.Failure(SeatErrors.AlreadyBooked);
            }

            await _seatLockService.ReleaseAsync(request.SeatIds, userId);

            var total = seats.Sum(s => s.Price);
            await _publisher.Publish(new SeatsPurchasedNotification(userId,request.EventId,request.SeatIds,total),cancellationToken);

            return Result.Success();
        }
    }
}
