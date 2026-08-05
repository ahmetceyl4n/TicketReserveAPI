using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Events.UpdateEvent
{
    public sealed record UpdateEventCommand(Guid Id, string Name, string Description, string Venue, DateTime Date)
        : IRequest<Result<EventSummary>>;

    public sealed record UpdateEventRequest(string Name, string Description, string Venue, DateTime Date);

}
