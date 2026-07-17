using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Seats.GetSeats;

public sealed record GetSeatsQuery(Guid EventId) : IRequest<Result<List<SeatSummary>>>;
