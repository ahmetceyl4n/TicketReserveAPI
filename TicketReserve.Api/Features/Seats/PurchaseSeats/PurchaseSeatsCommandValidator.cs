using FluentValidation;

namespace TicketReserve.Api.Features.Seats.PurchaseSeats;

public sealed class PurchaseSeatsCommandValidator : AbstractValidator<PurchaseSeatsCommand>
{
    public PurchaseSeatsCommandValidator()
    {
        RuleFor(x => x.SeatIds).NotEmpty();

        RuleFor(x => x.SeatIds)
            .Must(ids => ids.Count <= 10)
            .WithMessage("Tek seferde en fazla 10 koltuk satın alınabilir.");

        RuleFor(x => x.SeatIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Aynı koltuk birden fazla kez gönderilemez.");
    }
}
