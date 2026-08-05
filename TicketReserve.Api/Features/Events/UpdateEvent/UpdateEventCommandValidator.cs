using FluentValidation;

namespace TicketReserve.Api.Features.Events.UpdateEvent
{
    public sealed class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
    {
        public UpdateEventCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Venue).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Date).GreaterThan(DateTime.UtcNow).WithMessage("Etkinlik tarihi geçmişte olamaz.");
        }
    }
}
