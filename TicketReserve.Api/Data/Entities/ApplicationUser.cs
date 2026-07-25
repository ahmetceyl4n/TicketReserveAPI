using Microsoft.AspNetCore.Identity;

namespace TicketReserve.Api.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public required string FullName { get; set; }
}
