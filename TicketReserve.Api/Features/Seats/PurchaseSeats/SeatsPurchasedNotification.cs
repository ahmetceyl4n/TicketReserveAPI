using MediatR;


namespace TicketReserve.Api.Features.Seats.PurchaseSeats;

public sealed record SeatsPurchasedNotification(
        string UserId,
        Guid EventId,
        IReadOnlyList<Guid> SeatIds,
        decimal TotalPrice
        ):INotification ;
    

