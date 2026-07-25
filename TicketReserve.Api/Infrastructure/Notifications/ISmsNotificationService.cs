using TicketReserve.Api.Features.Seats.PurchaseSeats;

namespace TicketReserve.Api.Infrastructure.Notifications;

public interface ISmsNotificationService
{
    Task SendPurchaseConfirmationAsync(SeatsPurchasedIntegrationEvent e, CancellationToken ct);
}
