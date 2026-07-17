using FluentValidation;

namespace TicketReserve.Api.Features.Events.CreateEvent;

public sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Date)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Etkinlik tarihi geçmişte olamaz.");
    }
}
