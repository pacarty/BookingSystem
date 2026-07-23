using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs;

// DTOs are kept separate from entities on purpose: the API should never
// serialize EF Core entities directly (risk of leaking navigation
// properties, over-posting, lazy-loading proxies, etc).

public record CreateAppointmentRequest(
    Guid ProviderId,
    Guid ServiceId,
    Guid ClientId,
    DateTime StartUtc,
    string? Notes
);

public record AppointmentResponse(
    Guid Id,
    Guid ProviderId,
    string ProviderName,
    Guid ClientId,
    string ClientName,
    Guid ServiceId,
    string ServiceName,
    DateTime StartUtc,
    DateTime EndUtc,
    AppointmentStatus Status,
    string? Notes
);

public record UpdateAppointmentStatusRequest(AppointmentStatus Status);
