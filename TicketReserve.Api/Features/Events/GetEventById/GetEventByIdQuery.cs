using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Caching;

namespace TicketReserve.Api.Features.Events.GetEventById;

public sealed record GetEventByIdQuery(Guid Id) : IRequest<Result<EventSummary>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.Event(Id);
};
