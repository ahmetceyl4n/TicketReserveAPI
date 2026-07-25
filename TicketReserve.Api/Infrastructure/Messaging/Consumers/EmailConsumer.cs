using MassTransit;
using TicketReserve.Api.Features.Auth.Register;
using TicketReserve.Api.Infrastructure.Notifications;
using TicketReserve.Api.Features.Seats.PurchaseSeats;

namespace TicketReserve.Api.Infrastructure.Messaging.Consumers;

// İnce adaptör: mesajı karşılar, işi servise devreder. Deserialize/Inbox/ack/log yok — hepsi
// MassTransit pipeline'ında + IEmailNotificationService'te.
public sealed class EmailConsumer :
    IConsumer<SeatsPurchasedIntegrationEvent>,
    IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly IEmailNotificationService _email;

    public EmailConsumer(IEmailNotificationService email)
    {
        _email = email;
    }

    public Task Consume(ConsumeContext<SeatsPurchasedIntegrationEvent> context)
        => _email.SendPurchaseConfirmationAsync(context.Message, context.CancellationToken);

    public Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
        => _email.SendWelcomeAsync(context.Message, context.CancellationToken);
}
