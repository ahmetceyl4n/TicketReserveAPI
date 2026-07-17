using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Seats.ReleaseSeats;

public sealed record ReleaseSeatsCommand(Guid EventId, List<Guid> SeatIds) : IRequest<Result>;

public sealed record ReleaseSeatsRequest(List<Guid> SeatIds);
