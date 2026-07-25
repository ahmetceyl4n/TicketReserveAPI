using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;

namespace TicketReserve.Api.Features.Events.DeleteEvent;

public sealed class DeleteEventHandler : IRequestHandler<DeleteEventCommand, Result>
{
    private readonly ApplicationDbContext _dbContext;

    public DeleteEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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

        return Result.Success();
    }
}
