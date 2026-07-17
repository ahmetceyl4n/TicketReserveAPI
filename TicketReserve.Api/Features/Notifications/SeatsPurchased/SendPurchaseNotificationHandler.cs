using MediatR;
using TicketReserve.Api.Features.Seats.PurchaseSeats;

namespace TicketReserve.Api.Features.Notifications.SeatsPurchased;

public sealed class SendPurchaseNotificationHandler : INotificationHandler<SeatsPurchasedNotification>
{
    private readonly ILogger<SendPurchaseNotificationHandler> _logger;

    public SendPurchaseNotificationHandler(ILogger<SendPurchaseNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(SeatsPurchasedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Mail gönderildi → Kullanıcı {UserId}, Etkinlik {EventId}, {SeatCount} koltuk, toplam {TotalPrice}",
            notification.UserId,
            notification.EventId,
            notification.SeatIds.Count,
            notification.TotalPrice);

        return Task.CompletedTask;
    }
}
