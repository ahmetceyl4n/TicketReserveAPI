using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Seats.LockSeats
{
    public sealed record LockSeatsCommand(Guid EventId, List<Guid> SeatIds) : IRequest<Result<LockSeatsResponse>>;

    public sealed record LockSeatsRequest(List<Guid> SeatIds);

    public sealed record LockSeatsResponse(List<Guid> LockedSeatIds, DateTime ExpiresAtUtc);

}
