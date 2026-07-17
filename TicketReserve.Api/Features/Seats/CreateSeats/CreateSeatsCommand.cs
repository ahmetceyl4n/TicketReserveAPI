using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Seats.CreateSeats;

public sealed record CreateSeatsCommand(Guid EventId, string StartRow, string EndRow, int SeatsPerRow, decimal Price)
    : IRequest<Result<List<SeatSummary>>>;

// EventId route'tan geldiği için body DTO'sunda yok — Command ile birebir aynı değil.
public sealed record CreateSeatsRequest(string StartRow, string EndRow, int SeatsPerRow, decimal Price);
