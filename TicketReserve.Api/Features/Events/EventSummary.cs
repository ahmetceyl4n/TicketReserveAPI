namespace TicketReserve.Api.Features.Events;

public sealed record EventSummary(Guid Id, string Name, string Description, string Venue, DateTime Date, string? ImageUrl);
