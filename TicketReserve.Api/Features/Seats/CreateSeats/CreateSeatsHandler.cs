using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Features.Events;

namespace TicketReserve.Api.Features.Seats.CreateSeats;

public sealed class CreateSeatsHandler : IRequestHandler<CreateSeatsCommand, Result<List<SeatSummary>>>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateSeatsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<SeatSummary>>> Handle(CreateSeatsCommand request, CancellationToken cancellationToken)
    {
        var eventExists = await _dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken);
        if (!eventExists)
        {
            return Result.Failure<List<SeatSummary>>(EventErrors.NotFound);
        }

        var startRow = char.ToUpperInvariant(request.StartRow[0]);
        var endRow = char.ToUpperInvariant(request.EndRow[0]);
        var rows = Enumerable.Range(startRow, endRow - startRow + 1)
            .Select(code => ((char)code).ToString())
            .ToArray();

        var hasExistingSeats = await _dbContext.Seats
            .AnyAsync(s => s.EventId == request.EventId && rows.Contains(s.Row), cancellationToken);

        if (hasExistingSeats)
        {
            return Result.Failure<List<SeatSummary>>(SeatErrors.AlreadyExists);
        }

        var newSeats = rows
            .SelectMany(row => Enumerable.Range(1, request.SeatsPerRow)
                .Select(number => new Seat
                {
                    Id = Guid.NewGuid(),
                    EventId = request.EventId,
                    Row = row,
                    Number = number,
                    Price = request.Price,
                    Status = SeatStatus.Available
                }))
            .ToList();

        _dbContext.Seats.AddRange(newSeats);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdSeats = newSeats
            .Select(seat => new SeatSummary(seat.Id, seat.Row, seat.Number, seat.Price, SeatViewStatus.Available))
            .ToList();

        return Result.Success(createdSeats);
    }
}
