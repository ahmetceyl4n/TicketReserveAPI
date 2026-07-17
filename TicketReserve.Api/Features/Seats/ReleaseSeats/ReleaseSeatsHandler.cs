using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Auth;
using TicketReserve.Api.Infrastructure.Locking;

namespace TicketReserve.Api.Features.Seats.ReleaseSeats;

public sealed class ReleaseSeatsHandler : IRequestHandler<ReleaseSeatsCommand, Result>
{
    private readonly ISeatLockService _seatLockService;
    private readonly ICurrentUserService _currentUser;

    public ReleaseSeatsHandler(ISeatLockService seatLockService, ICurrentUserService currentUser)
    {
        _seatLockService = seatLockService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(ReleaseSeatsCommand request, CancellationToken cancellationToken)
    {
        // Idempotent: kilit zaten sende değilse (süresi dolmuş ya da hiç alınmamışsa) de 200
        // dönüyoruz — kullanıcının "vazgeç" niyeti zaten gerçekleşmiş durumda, hata değil.
        await _seatLockService.ReleaseAsync(request.SeatIds, _currentUser.UserId);

        return Result.Success();
    }
}
