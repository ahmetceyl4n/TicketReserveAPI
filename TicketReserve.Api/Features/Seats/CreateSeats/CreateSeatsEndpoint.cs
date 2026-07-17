using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Seats.CreateSeats;

public sealed class CreateSeatsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/events/{eventId:guid}/seats", async (Guid eventId, CreateSeatsRequest request, IMediator mediator) =>
            {
                var command = new CreateSeatsCommand(
                    eventId,
                    request.StartRow,
                    request.EndRow,
                    request.SeatsPerRow,
                    request.Price);

                var result = await mediator.Send(command);
                return result.ToHttpResult();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}
