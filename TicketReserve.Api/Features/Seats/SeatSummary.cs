
namespace TicketReserve.Api.Features.Seats;

public enum SeatViewStatus
{
    Available,
    Locked,
    Sold
};
public sealed record SeatSummary(Guid Id, string Row, int Number, decimal Price, SeatViewStatus Status);
