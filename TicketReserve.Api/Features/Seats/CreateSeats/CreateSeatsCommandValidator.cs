using FluentValidation;

namespace TicketReserve.Api.Features.Seats.CreateSeats;

public sealed class CreateSeatsCommandValidator : AbstractValidator<CreateSeatsCommand>
{
    public CreateSeatsCommandValidator()
    {
        RuleFor(x => x.StartRow).NotEmpty().Length(1).Matches("^[A-Za-z]$");
        RuleFor(x => x.EndRow).NotEmpty().Length(1).Matches("^[A-Za-z]$");
        RuleFor(x => x)
            .Must(x => string.Compare(x.StartRow, x.EndRow, StringComparison.OrdinalIgnoreCase) <= 0)
            .WithMessage("Bitiş satırı, başlangıç satırından önce olamaz.")
            .WithName("EndRow");
        RuleFor(x => x.SeatsPerRow).InclusiveBetween(1, 100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
