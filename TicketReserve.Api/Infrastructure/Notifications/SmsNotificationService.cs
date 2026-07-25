using TicketReserve.Api.Features.Seats.PurchaseSeats;

namespace TicketReserve.Api.Infrastructure.Notifications;

public sealed class SmsNotificationService : ISmsNotificationService
{
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(ILogger<SmsNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendPurchaseConfirmationAsync(SeatsPurchasedIntegrationEvent e, CancellationToken ct)
    {
        _logger.LogInformation(
            "SMS gönderildi → Kullanıcı {UserId}, Etkinlik {EventId}, {SeatCount} koltuk, toplam {TotalPrice}",
            e.UserId, e.EventId, e.SeatIds.Count, e.TotalPrice);
        return Task.CompletedTask;
    }
}
