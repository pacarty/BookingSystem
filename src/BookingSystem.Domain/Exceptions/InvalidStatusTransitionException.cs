using BookingSystem.Domain.Enums;

namespace BookingSystem.Domain.Exceptions;

public class InvalidStatusTransitionException : Exception
{
    public InvalidStatusTransitionException(AppointmentStatus from, AppointmentStatus to)
        : base($"Cannot move an appointment from {from} to {to}.") { }
}