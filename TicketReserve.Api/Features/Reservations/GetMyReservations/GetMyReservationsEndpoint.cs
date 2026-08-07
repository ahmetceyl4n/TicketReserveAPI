using MediatR;
using TicketReserve.Api.Common;
using TicketReserve.Api.Infrastructure.Endpoints;

namespace TicketReserve.Api.Features.Reservations.GetMyReservations
{
    public class GetMyReservationsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/reservations/me", async (ISender mediator) =>
                (await mediator.Send(new GetMyReservationsQuery())).ToHttpResult())
                .RequireAuthorization();
        }
    }
}
