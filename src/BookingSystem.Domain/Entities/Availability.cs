using BookingSystem.Domain.Common;

namespace BookingSystem.Domain.Entities;

// A recurring weekly working-hours block, e.g. "Tuesdays 09:00-17:00".
// The bookable time slots shown to clients are computed at query time by
// taking these blocks and subtracting existing Appointments - they are
// never materialised as rows themselves.
public class Availability : BaseEntity
{
    public Guid ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
