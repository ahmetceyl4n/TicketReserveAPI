using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Seats;

public static class SeatErrors
{
    public static readonly Error AlreadyExists = new(
        "Seat.AlreadyExists",
        "Belirtilen aralıkta en az bir koltuk zaten var.",
        ErrorType.Conflict);

    public static readonly Error AlreadyLocked = new(
        "Seat.AlreadyLocked",
        "Koltuklardan en az biri şu anda başka bir kullanıcı tarafından kilitli.",
        ErrorType.Conflict);

    public static readonly Error LockNotOwned = new(
        "Seat.LockNotOwned",
        "Koltuklardan en az birinin kilidi sende değil ya da süresi dolmuş.",
        ErrorType.Conflict);

    public static readonly Error AlreadyBooked = new(
        "Seat.AlreadyBooked",
        "Koltuklardan en az biri zaten satılmış.",
        ErrorType.Conflict);

    public static readonly Error NotFound = new(
        "Seat.NotFound",
        "Koltuklardan en az biri bu etkinlikte bulunamadı.",
        ErrorType.NotFound);
}
