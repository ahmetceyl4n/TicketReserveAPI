using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Auth;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials = new(
        "Auth.InvalidCredentials",
        "E-posta veya şifre hatalı.",
        ErrorType.Unauthorized);

    public static readonly Error EmailAlreadyExists = new(
        "Auth.EmailAlreadyExists",
        "Bu e-posta adresiyle kayıtlı bir kullanıcı zaten var.",
        ErrorType.Conflict);
}
