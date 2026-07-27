using BookingSystem.Application.DTOs;
using BookingSystem.Application.Exceptions;
using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;

namespace BookingSystem.Application.Services;

// This is the class that would come up most in a code-review interview
// question: given a provider's recurring weekly hours and their existing
// appointments, work out what's actually bookable, and never allow two
// appointments to double-book the same provider.
public class AppointmentBookingService : IAppointmentBookingService
{
    private readonly IAppointmentRepository _appointments;
    private readonly IProviderRepository _providers;
    private readonly IServiceRepository _services;
    private readonly IClientRepository _clients;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentBookingService(
        IAppointmentRepository appointments,
        IProviderRepository providers,
        IServiceRepository services,
        IClientRepository clients,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _appointments = appointments;
        _providers = providers;
        _services = services;
        _clients = clients;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AvailableSlotResponse>> GetAvailableSlotsAsync(
        Guid providerId, Guid serviceId, DateOnly date, CancellationToken ct = default)
    {
        var service = await _services.GetByIdAsync(serviceId, ct)
            ?? throw new KeyNotFoundException($"Service {serviceId} not found.");

        var availability = await _providers.GetAvailabilityAsync(providerId, ct);
        var todaysWindow = availability.FirstOrDefault(a => a.DayOfWeek == date.DayOfWeek);
        if (todaysWindow is null)
        {
            return new List<AvailableSlotResponse>(); // provider doesn't work this day of week
        }

        var dayStartUtc = date.ToDateTime(todaysWindow.StartTime, DateTimeKind.Utc);
        var dayEndUtc = date.ToDateTime(todaysWindow.EndTime, DateTimeKind.Utc);
        var duration = TimeSpan.FromMinutes(service.DurationMinutes);

        var existing = await _appointments.GetForProviderBetweenAsync(providerId, dayStartUtc, dayEndUtc, ct);

        var slots = new List<AvailableSlotResponse>();
        for (var slotStart = dayStartUtc; slotStart + duration <= dayEndUtc; slotStart += duration)
        {
            var slotEnd = slotStart + duration;

            var overlapsExisting = existing.Any(a => slotStart < a.EndUtc && a.StartUtc < slotEnd);
            if (!overlapsExisting)
            {
                slots.Add(new AvailableSlotResponse(providerId, serviceId, slotStart, slotEnd));
            }
        }

        return slots;
    }

    public async Task<AppointmentResponse> BookAsync(CreateAppointmentRequest request, CancellationToken ct = default)
    {
        var provider = await _providers.GetByIdAsync(request.ProviderId, ct)
            ?? throw new KeyNotFoundException($"Provider {request.ProviderId} not found.");

        var service = await _services.GetByIdAsync(request.ServiceId, ct)
            ?? throw new KeyNotFoundException($"Service {request.ServiceId} not found.");

        var endUtc = request.StartUtc.AddMinutes(service.DurationMinutes);

        var availability = await _providers.GetAvailabilityAsync(request.ProviderId, ct);
        var window = availability.FirstOrDefault(a => a.DayOfWeek == request.StartUtc.DayOfWeek);
        var requestedStartTime = TimeOnly.FromDateTime(request.StartUtc);
        var requestedEndTime = TimeOnly.FromDateTime(endUtc);

        if (window is null || requestedStartTime < window.StartTime || requestedEndTime > window.EndTime)
        {
            throw new OutsideAvailabilityException(
                $"Provider is not available at {request.StartUtc:u}.");
        }

        // Re-check for conflicts at write time, not just when slots were displayed -
        // another booking could have landed in between the client viewing slots
        // and submitting this request.
        var hasOverlap = await _appointments.HasOverlapAsync(request.ProviderId, request.StartUtc, endUtc, ct);
        if (hasOverlap)
        {
            throw new BookingConflictException("This time slot was just booked by someone else. Please choose another.");
        }

        // Returning clients are matched by email rather than being asked to
        // "sign in" - there's no auth on the public site by design.
        var client = await _clients.GetByEmailAsync(request.ClientEmail, ct);
        if (client is null)
        {
            client = new Client
            {
                FirstName = request.ClientFirstName,
                LastName = request.ClientLastName,
                Email = request.ClientEmail,
                Phone = request.ClientPhone
            };
            await _clients.AddAsync(client, ct);
        }

        var appointment = new Appointment
        {
            ProviderId = request.ProviderId,
            ClientId = client.Id,
            ServiceId = request.ServiceId,
            StartUtc = request.StartUtc,
            EndUtc = endUtc,
            Notes = request.Notes
        };

        await _appointments.AddAsync(appointment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Fire-and-forget-ish: a failed notification shouldn't roll back a
        // successful booking. In a production system this would go on a
        // background queue (e.g. Azure Service Bus) rather than being awaited
        // inline - flagged here as the natural next step.
        await _notifications.SendAppointmentConfirmationAsync(appointment, ct);

        return new AppointmentResponse(
            appointment.Id,
            provider.Id,
            provider.Name,
            client.Id,
            $"{client.FirstName} {client.LastName}",
            service.Id,
            service.Name,
            appointment.StartUtc,
            appointment.EndUtc,
            appointment.Status,
            appointment.Notes);
    }
}
