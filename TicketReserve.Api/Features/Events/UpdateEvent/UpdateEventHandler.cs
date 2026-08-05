using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Infrastructure.Caching;

namespace TicketReserve.Api.Features.Events.UpdateEvent
{
    public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, Result<EventSummary>>
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly ICacheService _cache;
        public UpdateEventHandler(ApplicationDbContext dbContext, ICacheService cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<Result<EventSummary>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _dbContext.Events.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            
            if (eventEntity is null)
            {
                return  Result.Failure<EventSummary>(EventErrors.NotFound);
            }

            eventEntity.Name = request.Name;
            eventEntity.Description = request.Description;
            eventEntity.Venue = request.Venue;
            eventEntity.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _cache.RemoveAsync(CacheKeys.Event(request.Id),cancellationToken);

            return Result.Success(new EventSummary(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Description,
            eventEntity.Venue,
            eventEntity.Date,
            eventEntity.ImageUrl));
        }
    }
}
