using BookingSystem.Domain.Common;

namespace BookingSystem.Domain.Entities;

// A catalog entry, e.g. "Initial consultation - 45 min - $80".
// Kept separate from Provider so multiple providers can offer the same
// service (and even override its price) via ProviderService.
public class Service : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
