using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Infrastructure.Persistence.Configurations;

public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
{
    public void Configure(EntityTypeBuilder<Availability> builder)
    {
        builder.ToTable("Availabilities");

        builder.HasOne(a => a.Provider)
            .WithMany(p => p.Availabilities)
            .HasForeignKey(a => a.ProviderId);

        builder.HasIndex(a => new { a.ProviderId, a.DayOfWeek });
    }
}
