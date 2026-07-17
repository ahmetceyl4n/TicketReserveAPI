namespace TicketReserve.Api.Infrastructure.Auth;

public interface ICurrentUserService
{
    string UserId { get; }
}
