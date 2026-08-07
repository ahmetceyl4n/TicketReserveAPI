using MediatR;

namespace TicketReserve.Api.Infrastructure.Realtime
{
    public sealed record SeatStatusChangedNotification(
    Guid EventId,
    Guid SeatId,
    string Status) : INotification;
}
