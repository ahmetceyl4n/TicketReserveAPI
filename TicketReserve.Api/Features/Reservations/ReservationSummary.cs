namespace TicketReserve.Api.Features.Reservations
{
    public sealed record ReservationSummary(
    Guid ReservationId,
    Guid EventId,
    string EventName,
    string Venue,
    DateTime EventDate,
    string SeatRow,
    int SeatNumber,
    decimal Price,
    DateTime PurchasedAtUtc);
}
