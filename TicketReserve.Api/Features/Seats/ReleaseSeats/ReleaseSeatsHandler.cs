using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Auth;
using TicketReserve.Api.Infrastructure.Locking;
using TicketReserve.Api.Infrastructure.Realtime;

namespace TicketReserve.Api.Features.Seats.ReleaseSeats;

public sealed class ReleaseSeatsHandler : IRequestHandler<ReleaseSeatsCommand, Result>
{
    private readonly ISeatLockService _seatLockService;
    private readonly ICurrentUserService _currentUser;
    private readonly IPublisher _publisher;

    public ReleaseSeatsHandler(ISeatLockService seatLockService, ICurrentUserService currentUser, IPublisher publisher)
    {
        _seatLockService = seatLockService;
        _currentUser = currentUser;
        _publisher = publisher;
    }

    public async Task<Result> Handle(ReleaseSeatsCommand request, CancellationToken cancellationToken)
    {
        // Idempotent: kilit zaten sende değilse (süresi dolmuş ya da hiç alınmamışsa) de 200
        // dönüyoruz — kullanıcının "vazgeç" niyeti zaten gerçekleşmiş durumda, hata değil.
        await _seatLockService.ReleaseAsync(request.SeatIds, _currentUser.UserId);

        foreach (var seatId in request.SeatIds)
        {
            await _publisher.Publish(
                new SeatStatusChangedNotification(request.EventId, seatId, "Available"),
                cancellationToken);
        }

        return Result.Success();
    }
}
