using TicketReserve.Api.Features.Auth.Register;
using TicketReserve.Api.Features.Seats.PurchaseSeats;

namespace TicketReserve.Api.Infrastructure.Notifications;

public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ILogger<EmailNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendPurchaseConfirmationAsync(SeatsPurchasedIntegrationEvent e, CancellationToken ct)
    {
        _logger.LogInformation(
            "Mail gönderildi → Kullanıcı {UserId}, Etkinlik {EventId}, {SeatCount} koltuk, toplam {TotalPrice}",
            e.UserId, e.EventId, e.SeatIds.Count, e.TotalPrice);
        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(UserRegisteredIntegrationEvent e, CancellationToken ct)
    {
        _logger.LogInformation("Kayıt maili gönderildi → {Email}, {FullName}", e.Email, e.FullName);
        return Task.CompletedTask;
    }
}
