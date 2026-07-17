using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Events.GetEventById;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<Result<EventSummary>>;
