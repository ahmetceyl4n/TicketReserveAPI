using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Events;

public static class EventErrors
{
    public static readonly Error NotFound = new(
        "Event.NotFound",
        "Etkinlik bulunamadı.",
        ErrorType.NotFound);
}
