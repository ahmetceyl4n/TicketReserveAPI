using TicketReserve.Api.Features.Auth.Register;
using TicketReserve.Api.Features.Seats.PurchaseSeats;

namespace TicketReserve.Api.Infrastructure.Notifications;

public interface IEmailNotificationService
{
    Task SendPurchaseConfirmationAsync(SeatsPurchasedIntegrationEvent e, CancellationToken ct);
    Task SendWelcomeAsync(UserRegisteredIntegrationEvent e, CancellationToken ct);
}
