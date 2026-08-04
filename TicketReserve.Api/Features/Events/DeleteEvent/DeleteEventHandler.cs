using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;
using TicketReserve.Api.Infrastructure.Caching;

namespace TicketReserve.Api.Features.Events.DeleteEvent;

public sealed class DeleteEventHandler : IRequestHandler<DeleteEventCommand, Result>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICacheService _cache;

    public DeleteEventHandler(ApplicationDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = await _dbContext.Events
            .Include(e => e.Seats)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (eventEntity is null)
        {
            return Result.Failure(EventErrors.NotFound);
        }

        // Gerçek bir DELETE üretmiyor — SoftDeleteInterceptor bunu yakalayıp
        // IsDeleted=true'ya çeviriyor (Event + Include ile tracked olan, cascade
        // sayesinde otomatik Deleted işaretlenen Seats için de).
        _dbContext.Events.Remove(eventEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Silme sonrası eski kopyayı at — yoksa GET, TTL boyunca silinmiş etkinliği cache'ten döndürür.
        await _cache.RemoveAsync(CacheKeys.Event(request.Id), cancellationToken);

        return Result.Success();
    }
}
