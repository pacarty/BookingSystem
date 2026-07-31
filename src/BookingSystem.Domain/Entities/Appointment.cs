using BookingSystem.Domain.Common;
using BookingSystem.Domain.Enums;
using BookingSystem.Domain.Exceptions;

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

    public AppointmentStatus Status { get; private set; } = AppointmentStatus.Requested;
    public string? Notes { get; set; }
    public DateTime? ConfirmedUtc { get; set; }
    public DateTime? CancelledUtc { get; set; }

    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();

    // The only way Status should change after creation. Making the setter
    // private above and routing every change through here means "which
    // transitions are legal" lives in one place, and can never be
    // bypassed by a controller (or a future one) just assigning the
    // property directly.
    public void UpdateStatus(AppointmentStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
        {
            throw new InvalidStatusTransitionException(Status, newStatus);
        }

        Status = newStatus;
        UpdatedUtc = DateTime.UtcNow;

        if (newStatus == AppointmentStatus.Confirmed) ConfirmedUtc = DateTime.UtcNow;
        if (newStatus == AppointmentStatus.Cancelled) CancelledUtc = DateTime.UtcNow;
    }

    private static bool IsValidTransition(AppointmentStatus from, AppointmentStatus to) => (from, to) switch
    {
        (AppointmentStatus.Requested, AppointmentStatus.Confirmed) => true,
        (AppointmentStatus.Requested, AppointmentStatus.Cancelled) => true,
        (AppointmentStatus.Confirmed, AppointmentStatus.Attended) => true,
        (AppointmentStatus.Confirmed, AppointmentStatus.NoShow) => true,
        (AppointmentStatus.Confirmed, AppointmentStatus.Cancelled) => true,
        _ => false // Attended, NoShow, and Cancelled are all terminal - nothing moves out of them
    };
}
