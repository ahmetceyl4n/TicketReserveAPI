using Microsoft.AspNetCore.SignalR;

namespace TicketReserve.Api.Infrastructure.Realtime
{
    public sealed class SeatHub : Hub
    {
        public async Task JoinEventGroup(string eventId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, eventId);
        }
    }
}
