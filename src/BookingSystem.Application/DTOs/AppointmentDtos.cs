using BookingSystem.Domain.Enums;

namespace BookingSystem.Application.DTOs;

// DTOs are kept separate from entities on purpose: the API should never
// serialize EF Core entities directly (risk of leaking navigation
// properties, over-posting, lazy-loading proxies, etc).

// The public site never asks a client to sign up first - it collects their
// details as part of the booking form. AppointmentBookingService finds an
// existing Client by email, or creates one, rather than requiring a
// pre-existing ClientId.
public record CreateAppointmentRequest(
    Guid ProviderId,
    Guid ServiceId,
    string ClientFirstName,
    string ClientLastName,
    string ClientEmail,
    string ClientPhone,
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
