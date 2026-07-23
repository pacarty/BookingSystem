using BookingSystem.Domain.Common;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    // Stored in UTC; converted to the provider's local time only at the
    // presentation layer. This avoids daylight-saving bugs when computing
    // availability and conflict checks.
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;
    public string? Notes { get; set; }
    public DateTime? ConfirmedUtc { get; set; }
    public DateTime? CancelledUtc { get; set; }

    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
