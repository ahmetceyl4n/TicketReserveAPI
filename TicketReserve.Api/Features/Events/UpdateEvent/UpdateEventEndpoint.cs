using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Events.UpdateEvent
{
    public class UpdateEventEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/events/{id:guid}", async (Guid id, UpdateEventRequest request, IMediator mediator) =>
            {
                var command = new UpdateEventCommand(id, request.Name, request.Description, request.Venue, request.Date);
                var result = await mediator.Send(command);

                return result.ToHttpResult();
            })
                .RequireAuthorization(policy => policy.RequireRole("Admin"));
        }
    }
}
