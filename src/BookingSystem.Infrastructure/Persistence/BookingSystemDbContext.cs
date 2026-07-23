using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Persistence;

public class BookingSystemDbContext : DbContext
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
        // Keeping entity configuration in separate IEntityTypeConfiguration<T>
        // classes (rather than inline here) is the pattern EF Core docs and
        // most real codebases use - it's worth being able to speak to this
        // choice in an interview.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingSystemDbContext).Assembly);
    }
}
