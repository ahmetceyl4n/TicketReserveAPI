using TicketReserve.Api.Data;

namespace TicketReserve.Api.Infrastructure.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
