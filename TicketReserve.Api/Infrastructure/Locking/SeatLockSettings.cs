namespace TicketReserve.Api.Infrastructure.Locking
{
    public sealed class SeatLockSettings
    {
        public int TtlSeconds { get; init; } = 600;
    }
}
