using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Infrastructure.Persistence.Seed;

// Runs once at startup in Development only (wired up in Program.cs) so a
// freshly cloned copy of this project has something to click through
// immediately, instead of an empty database. None of this runs outside
// Development - a real deployment seeds accounts through an admin
// workflow, not a hardcoded password in source control.
public static class DevelopmentSeeder
{
    public const string DemoAdminEmail = "admin@bookingsystem.local";
    public const string DemoProviderEmail = "provider@bookingsystem.local";
    public const string DemoPassword = "Passw0rd!123"; // dev-only, intentionally simple

    public static async Task SeedAsync(
        BookingSystemDbContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger logger)
    {
        await db.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "Provider" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        if (!await db.Users.AnyAsync(u => u.Email == DemoAdminEmail))
        {
            var admin = new ApplicationUser
            {
                UserName = DemoAdminEmail,
                Email = DemoAdminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, DemoPassword);
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed one demo provider + a service they offer + their weekly hours,
        // so the public booking site has something real to show. Skipped
        // entirely if any Provider already exists.
        if (!await db.Providers.AnyAsync())
        {
            var provider = new Provider
            {
                Name = "Jordan Blake",
                Email = "jordan.blake@bookingsystem.local",
                Phone = "0400 123 456",
                Bio = "Independent consultant offering initial and follow-up sessions.",
                IsActive = true
            };

            var service = new Service
            {
                Name = "Initial Consultation",
                Description = "A 45-minute introductory session.",
                DurationMinutes = 45,
                Price = 120.00m,
                IsActive = true
            };

            var followUp = new Service
            {
                Name = "Follow-up Session",
                Description = "A 30-minute follow-up appointment.",
                DurationMinutes = 30,
                Price = 80.00m,
                IsActive = true
            };

            db.Providers.Add(provider);
            db.Services.AddRange(service, followUp);

            db.ProviderServices.Add(new ProviderService { Provider = provider, Service = service });
            db.ProviderServices.Add(new ProviderService { Provider = provider, Service = followUp });

            // Monday-Friday, 9am-5pm.
            foreach (var day in new[]
                     {
                         DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                         DayOfWeek.Thursday, DayOfWeek.Friday
                     })
            {
                db.Availabilities.Add(new Availability
                {
                    Provider = provider,
                    DayOfWeek = day,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0)
                });
            }

            await db.SaveChangesAsync();

            var providerUser = new ApplicationUser
            {
                UserName = DemoProviderEmail,
                Email = DemoProviderEmail,
                EmailConfirmed = true,
                ProviderId = provider.Id
            };
            await userManager.CreateAsync(providerUser, DemoPassword);
            await userManager.AddToRoleAsync(providerUser, "Provider");

            logger.LogInformation(
                "Seeded demo provider {ProviderName} with login {Email}", provider.Name, DemoProviderEmail);
        }

        logger.LogInformation(
            "Development seed complete. Admin login: {AdminEmail} / {Password}", DemoAdminEmail, DemoPassword);
    }
}
