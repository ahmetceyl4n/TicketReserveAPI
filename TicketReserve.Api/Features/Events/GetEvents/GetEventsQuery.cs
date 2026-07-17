using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Events.GetEvents;

public sealed record GetEventsQuery : IRequest<Result<List<EventSummary>>>;
