namespace BookingSystem.Domain.Enums;

public enum AppointmentStatus
{
    Requested = 0,   // client submitted a booking, awaiting confirmation
    Confirmed = 1,   // provider/system confirmed the slot
    Attended = 2,    // client showed up - terminal state
    NoShow = 3,      // client didn't show up - terminal state
    Cancelled = 4    // cancelled by client or provider before the appointment - terminal state
}
