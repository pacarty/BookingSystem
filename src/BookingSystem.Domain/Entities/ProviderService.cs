namespace BookingSystem.Domain.Entities;

// Many-to-many join between Provider and Service. Not every provider offers
// every service, and a provider may charge a different rate than the
// catalog default - hence PriceOverride rather than reusing Service.Price.
public class ProviderService
{
    public Guid ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public decimal? PriceOverride { get; set; }
}
