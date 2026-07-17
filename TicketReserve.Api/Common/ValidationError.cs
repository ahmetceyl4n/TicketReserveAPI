namespace TicketReserve.Api.Common;

public sealed record ValidationError(Error[] Errors)
    : Error("Validation.General", "Bir veya daha fazla doğrulama hatası oluştu.", ErrorType.Validation)
{
    public static ValidationError FromErrors(IEnumerable<Error> errors) => new(errors.ToArray());
}
