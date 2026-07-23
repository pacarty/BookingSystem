using BookingSystem.Domain.Common;

namespace BookingSystem.Domain.Entities;

// A provider is whoever the client is booking time with (consultant, tutor,
// trainer, stylist - deliberately generic). Renamed from "healthcare
// professional" in the original system, but the shape is identical.
public class Provider : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
