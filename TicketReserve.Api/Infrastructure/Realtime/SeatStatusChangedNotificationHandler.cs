using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace TicketReserve.Api.Infrastructure.Realtime
{
    public class SeatStatusChangedNotificationHandler : INotificationHandler<SeatStatusChangedNotification>
    {
        private readonly IHubContext<SeatHub> _hubContext;

        public SeatStatusChangedNotificationHandler(IHubContext<SeatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Handle(SeatStatusChangedNotification notification, CancellationToken cancellationToken)
        {
            await _hubContext.Clients
                .Group(notification.EventId.ToString())
                .SendAsync("SeatStatusChanged", notification, cancellationToken);
        }
    }
}
