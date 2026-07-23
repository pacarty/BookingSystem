namespace BookingSystem.Application.Exceptions;

public class BookingConflictException : Exception
{
    public BookingConflictException(string message) : base(message) { }
}

public class OutsideAvailabilityException : Exception
{
    public OutsideAvailabilityException(string message) : base(message) { }
}
