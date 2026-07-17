using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Features.Events;
using TicketReserve.Api.Infrastructure.Locking;

namespace TicketReserve.Api.Features.Seats.GetSeats;

public sealed class GetSeatsHandler : IRequestHandler<GetSeatsQuery, Result<List<SeatSummary>>>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISeatLockService _seatLockService;

    public GetSeatsHandler(ApplicationDbContext dbContext, ISeatLockService seatLockService)
    {
        _dbContext = dbContext;
        _seatLockService = seatLockService;
    }

    public async Task<Result<List<SeatSummary>>> Handle(GetSeatsQuery request, CancellationToken cancellationToken)
    {
        var eventExists = await _dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken);
        if (!eventExists)
        {
            return Result.Failure<List<SeatSummary>>(EventErrors.NotFound);
        }

        var seats = await _dbContext.Seats
            .AsNoTracking()
            .Where(s => s.EventId == request.EventId)
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Number)
            .ToListAsync(cancellationToken);

        var seatIds = seats.Select(s => s.Id).ToList();
        var lockedSeatIds = await _seatLockService.GetLockedSeatIdsAsync(seatIds);

        var seatSummaries = seats
            .Select(seat => new SeatSummary(
                seat.Id,
                seat.Row,
                seat.Number,
                seat.Price,
                GetViewStatus(seat, lockedSeatIds)))
            .ToList();

        return Result.Success(seatSummaries);
    }

    private static SeatViewStatus GetViewStatus(Seat seat, IReadOnlySet<Guid> lockedSeatIds)
    {
        if (seat.Status == SeatStatus.Sold)
        {
            return SeatViewStatus.Sold;
        }

        if (lockedSeatIds.Contains(seat.Id))
        {
            return SeatViewStatus.Locked;
        }

        return SeatViewStatus.Available;
    }
}
