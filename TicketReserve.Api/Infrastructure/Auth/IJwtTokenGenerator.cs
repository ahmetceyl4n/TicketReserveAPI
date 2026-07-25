using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;

namespace TicketReserve.Api.Infrastructure.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
