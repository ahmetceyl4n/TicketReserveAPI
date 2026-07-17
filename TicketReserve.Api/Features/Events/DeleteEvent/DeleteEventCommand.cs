using MediatR;
using TicketReserve.Api.Common;

namespace TicketReserve.Api.Features.Events.DeleteEvent;

public sealed record DeleteEventCommand(Guid Id) : IRequest<Result>;
