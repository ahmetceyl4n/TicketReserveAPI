using MassTransit;
using TicketReserve.Api.Infrastructure.Notifications;
using TicketReserve.Api.Features.Seats.PurchaseSeats;

namespace TicketReserve.Api.Infrastructure.Messaging.Consumers;

public sealed class SmsConsumer : IConsumer<SeatsPurchasedIntegrationEvent>
{
    private readonly ISmsNotificationService _sms;

    public SmsConsumer(ISmsNotificationService sms)
    {
        _sms = sms;
    }

    public Task Consume(ConsumeContext<SeatsPurchasedIntegrationEvent> context)
        => _sms.SendPurchaseConfirmationAsync(context.Message, context.CancellationToken);
}
