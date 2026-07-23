using BookingSystem.Application.DTOs;

namespace BookingSystem.Application.Services;

public interface IAppointmentBookingService
{
    Task<AppointmentResponse> BookAsync(CreateAppointmentRequest request, CancellationToken ct = default);

    Task<List<AvailableSlotResponse>> GetAvailableSlotsAsync(
        Guid providerId, Guid serviceId, DateOnly date, CancellationToken ct = default);
}
