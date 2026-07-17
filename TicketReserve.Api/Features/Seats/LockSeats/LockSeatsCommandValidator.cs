using FluentValidation;

namespace TicketReserve.Api.Features.Seats.LockSeats
{
    public sealed class LockSeatsCommandValidator : AbstractValidator<LockSeatsCommand>
    {
        public LockSeatsCommandValidator()
        {
            RuleFor(x => x.SeatIds).NotEmpty();

            RuleFor(x => x.SeatIds)
                .Must(ids => ids.Count <= 10)
                .WithMessage("Tek seferde en fazla 10 koltuk kilitlenebilir.");

            RuleFor(x => x.SeatIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Aynı koltuk birden fazla kez gönderilemez.");
        }
    }
}
