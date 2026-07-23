namespace BookingSystem.Domain.Enums;

public enum NotificationChannel
{
    Email = 0,
    Sms = 1
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
