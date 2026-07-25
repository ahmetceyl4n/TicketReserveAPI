using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;

namespace TicketReserve.Api.Features.Events.GetEventById;

public sealed class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, Result<EventSummary>>
{
    private readonly ApplicationDbContext _dbContext;

    public GetEventByIdHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<EventSummary>> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var eventSummary = await _dbContext.Events
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .Select(e => new EventSummary(e.Id, e.Name, e.Description, e.Venue, e.Date, e.ImageUrl))
            .FirstOrDefaultAsync(cancellationToken);

        return eventSummary is not null
            ? Result.Success(eventSummary)
            : Result.Failure<EventSummary>(EventErrors.NotFound);
    }
}
