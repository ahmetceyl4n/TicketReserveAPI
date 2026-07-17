using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Events.GetEventById;

public sealed class GetEventByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/events/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetEventByIdQuery(id));
            return result.ToHttpResult();
        });
    }
}
