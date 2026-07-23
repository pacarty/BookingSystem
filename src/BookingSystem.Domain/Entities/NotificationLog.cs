using BookingSystem.Domain.Common;
using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Entities;

// One row per notification attempt (confirmation email, reminder SMS, etc).
// Having this as its own table from day one means adding a real SMS
// provider later is purely an Infrastructure-layer change - the domain
// and API already expect notifications to be tracked and queryable.
public class NotificationLog : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime? SentUtc { get; set; }
}
