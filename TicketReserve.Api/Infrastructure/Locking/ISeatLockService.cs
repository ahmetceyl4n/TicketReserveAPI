namespace TicketReserve.Api.Infrastructure.Locking;

public interface ISeatLockService
{
    Task<bool> TryLockAsync(IReadOnlyCollection<Guid> seatIds, string userId, TimeSpan ttl);

    Task<bool> ReleaseAsync(IReadOnlyCollection<Guid> seatIds, string userId);

    Task<bool> AreAllLockedByAsync(IReadOnlyCollection<Guid> seatIds, string userId);

    Task<IReadOnlySet<Guid>> GetLockedSeatIdsAsync(IReadOnlyCollection<Guid> seatIds);
}
