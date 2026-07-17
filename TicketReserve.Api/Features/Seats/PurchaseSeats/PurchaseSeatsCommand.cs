using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Seats.PurchaseSeats;

public sealed record PurchaseSeatsCommand(Guid EventId, List<Guid> SeatIds) : IRequest<Result>;

public sealed record PurchaseSeatsRequest(List<Guid> SeatIds);
