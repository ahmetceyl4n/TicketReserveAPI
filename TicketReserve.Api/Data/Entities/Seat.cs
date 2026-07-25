namespace TicketReserve.Api.Data.Entities;

public enum SeatStatus
{
    Available,
    Sold
}

public class Seat : ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public required string Row { get; set; }
    public int Number { get; set; }
    public decimal Price { get; set; }
    public SeatStatus Status { get; set; } = SeatStatus.Available;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    // IsRowVersion() ile işaretlenip Postgres'in "xmin" sistem kolonuna eşleniyor (ApplicationDbContext).
    // Elle set edilmez; EF Core her UPDATE'te WHERE Version = <okunan değer> ekler, satır arada
    // değiştiyse 0 satır etkilenir ve DbUpdateConcurrencyException fırlatılır.
    public uint Version { get; set; }

    public Event Event { get; set; } = null!;
}
