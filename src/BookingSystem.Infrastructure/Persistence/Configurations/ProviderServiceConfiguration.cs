using BookingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingSystem.Infrastructure.Persistence.Configurations;

public class ProviderServiceConfiguration : IEntityTypeConfiguration<ProviderService>
{
    public void Configure(EntityTypeBuilder<ProviderService> builder)
    {
        builder.ToTable("ProviderServices");
        builder.HasKey(ps => new { ps.ProviderId, ps.ServiceId });
        builder.Property(ps => ps.PriceOverride).HasColumnType("decimal(10,2)");

        builder.HasOne(ps => ps.Provider)
            .WithMany(p => p.ProviderServices)
            .HasForeignKey(ps => ps.ProviderId);

        builder.HasOne(ps => ps.Service)
            .WithMany(s => s.ProviderServices)
            .HasForeignKey(ps => ps.ServiceId);
    }
}
