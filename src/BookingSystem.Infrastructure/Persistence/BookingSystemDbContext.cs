using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Persistence;

// Inheriting IdentityDbContext gives us AspNetUsers/AspNetRoles/etc. in the
// same database as the domain tables below - simplest setup for a single
// API + two SPA clients. A larger system might split auth into its own
// store, but that's not a trade worth making here.
public class BookingSystemDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public BookingSystemDbContext(DbContextOptions<BookingSystemDbContext> options) : base(options) { }

    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ProviderService> ProviderServices => Set<ProviderService>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity's own model configuration must run first.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.HasOne(u => u.Provider)
                .WithMany()
                .HasForeignKey(u => u.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingSystemDbContext).Assembly);
    }
}
