using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Data;

namespace TicketReserve.Api.Features.Events.CreateEvent;

public sealed class CreateEventHandler : IRequestHandler<CreateEventCommand, Result<EventSummary>>
{
    private readonly ApplicationDbContext _dbContext;

    public CreateEventHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<EventSummary>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Venue = request.Venue,
            // Npgsql, sadece Kind=Utc olan DateTime'ları "timestamptz" olarak kabul eder —
            // frontend zaten ISO 8601 + "Z" gönderiyor (Kind=Utc gelir), ama burada
            // garantiye alıyoruz ki farklı bir istemci (ör. Swagger'dan elle test) kırılmasın.
            Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc),
            ImageUrl = null
        };

        _dbContext.Events.Add(eventEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new EventSummary(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Description,
            eventEntity.Venue,
            eventEntity.Date,
            eventEntity.ImageUrl));
    }
}
