using FluentValidation;

namespace TicketReserve.Api.Features.Seats.ReleaseSeats;

public sealed class ReleaseSeatsCommandValidator : AbstractValidator<ReleaseSeatsCommand>
{
    public ReleaseSeatsCommandValidator()
    {
        RuleFor(x => x.SeatIds).NotEmpty();

        RuleFor(x => x.SeatIds)
            .Must(ids => ids.Count <= 10)
            .WithMessage("Tek seferde en fazla 10 koltuk serbest bırakılabilir.");

        RuleFor(x => x.SeatIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Aynı koltuk birden fazla kez gönderilemez.");
    }
}
