using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;
using TicketReserve.Api.Infrastructure.Auth;
using TicketReserve.Api.Infrastructure.Locking;

namespace TicketReserve.Api.Features.Seats.LockSeats
{
    public sealed class LockSeatsHandler : IRequestHandler<LockSeatsCommand, Result<LockSeatsResponse>>
    {
        private readonly ApplicationDbContext _dbcontext;
        private readonly ISeatLockService _seatLockService;
        private readonly IOptions<SeatLockSettings> _settings;
        private readonly ICurrentUserService _currentUser;

        public LockSeatsHandler(
            ApplicationDbContext dbcontext,
            ISeatLockService seatLockService,
            IOptions<SeatLockSettings> locksettings,
            ICurrentUserService currentUser)
        {
            _dbcontext = dbcontext;
            _seatLockService = seatLockService;
            _settings = locksettings;
            _currentUser = currentUser;
        }

        public async Task<Result<LockSeatsResponse>> Handle(LockSeatsCommand request, CancellationToken cancellationToken)
        {
            var seats = await _dbcontext.Seats.Where(s => s.EventId == request.EventId && request.SeatIds.Contains(s.Id)).ToListAsync(cancellationToken);

            if (seats.Count != request.SeatIds.Count)
            {
                return Result.Failure<LockSeatsResponse>(SeatErrors.NotFound);
            }

            foreach (var seat in seats)
            {
                if (seat.Status != SeatStatus.Available)
                {
                    return Result.Failure<LockSeatsResponse>(SeatErrors.AlreadyBooked);
                }
            }

            var seatlock = await _seatLockService.TryLockAsync(request.SeatIds, _currentUser.UserId, TimeSpan.FromSeconds(_settings.Value.TtlSeconds));

            if (seatlock)
            {
                return Result.Success(new LockSeatsResponse(request.SeatIds, DateTime.UtcNow.AddSeconds(_settings.Value.TtlSeconds)));
            }

            return Result.Failure<LockSeatsResponse>(SeatErrors.AlreadyLocked);
        }
    }
}
