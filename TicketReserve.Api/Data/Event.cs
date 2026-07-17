namespace TicketReserve.Api.Data;

public class Event : ISoftDeletable
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Venue { get; set; }
    public DateTime Date { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<Seat> Seats { get; set; } = [];
}
