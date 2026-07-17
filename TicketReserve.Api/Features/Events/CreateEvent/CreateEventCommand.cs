using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Events.CreateEvent;

public sealed record CreateEventCommand(string Name, string Description, string Venue, DateTime Date)
    : IRequest<Result<EventSummary>>;
