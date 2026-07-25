using Microsoft.AspNetCore.Identity;
using TicketReserve.Api.Data;
using TicketReserve.Api.Data.Entities;

namespace TicketReserve.Api.Infrastructure.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        await EnsureRoleExistsAsync(roleManager, "Admin");
        await EnsureRoleExistsAsync(roleManager, "User");

        var existingAdmins = await userManager.GetUsersInRoleAsync("Admin");
        if (existingAdmins.Count > 0)
        {
            logger.LogInformation("Admin rolünde en az bir kullanıcı zaten var, seed admin oluşturulmayacak.");
            return;
        }

        var adminEmail = configuration["SeedAdmin:Email"];
        var adminPassword = configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "SeedAdmin:Email / SeedAdmin:Password user secrets'ta tanımlı değil.");
        }

        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Seed Admin",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Seed admin oluşturulamadı: {string.Join(" ", createResult.Errors.Select(error => error.Description))}");
        }

        await userManager.AddToRoleAsync(adminUser, "Admin");

        logger.LogInformation("Seed admin oluşturuldu: {Email}", adminEmail);
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
