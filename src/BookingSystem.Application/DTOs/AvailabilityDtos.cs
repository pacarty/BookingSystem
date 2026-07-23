namespace BookingSystem.Application.DTOs;

public record AvailableSlotResponse(
    Guid ProviderId,
    Guid ServiceId,
    DateTime StartUtc,
    DateTime EndUtc
);
