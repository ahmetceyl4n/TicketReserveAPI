namespace TicketReserve.Api.Features.Auth.Register
{
    public sealed record UserRegisteredIntegrationEvent(string UserId, string Email, string FullName)
    {
    }
}
