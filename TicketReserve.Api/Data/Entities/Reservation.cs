namespace TicketReserve.Api.Data.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid SeatId { get; set; }
    public required string UserId { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Seat Seat { get; set; } = null!;
}
