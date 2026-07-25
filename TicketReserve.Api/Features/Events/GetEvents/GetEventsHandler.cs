using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;

namespace TicketReserve.Api.Features.Events.GetEvents;

public sealed class GetEventsHandler : IRequestHandler<GetEventsQuery, Result<List<EventSummary>>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEventsHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<EventSummary>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _dbContext.Events
            .AsNoTracking()
            .OrderBy(e => e.Date)
            .Select(e => new EventSummary(e.Id, e.Name, e.Description, e.Venue, e.Date, e.ImageUrl))
            .ToListAsync(cancellationToken);

        return Result.Success(events);
    }
}
