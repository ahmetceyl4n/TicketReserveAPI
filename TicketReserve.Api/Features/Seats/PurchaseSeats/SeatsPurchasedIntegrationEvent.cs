namespace TicketReserve.Api.Features.Seats.PurchaseSeats;

// Integration Event: süreç/broker sınırını aşan olay (RabbitMQ'ya yayınlanıp ayrı consumer'larca tüketiliyor).
// Domain event'ten farkı: bu, servisin içinde kalmaz — dışarıya "koltuk satıldı" gerçeğini duyurur.
// Aynı zamanda Outbox mesajının JSON payload'u.
public sealed record SeatsPurchasedIntegrationEvent(
    string UserId,
    Guid EventId,
    IReadOnlyList<Guid> SeatIds,
    decimal TotalPrice);
