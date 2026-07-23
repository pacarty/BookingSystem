using BookingSystem.Application.DTOs;
using BookingSystem.Application.Exceptions;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using BookingSystem.Domain.Entities;
using Moq;
using Xunit;

namespace BookingSystem.UnitTests;

public class AppointmentBookingServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointments = new();
    private readonly Mock<IProviderRepository> _providers = new();
    private readonly Mock<IServiceRepository> _services = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AppointmentBookingService BuildService() => new(
        _appointments.Object, _providers.Object, _services.Object, _notifications.Object, _unitOfWork.Object);

    [Fact]
    public async Task BookAsync_Throws_WhenSlotOverlapsAnExistingAppointment()
    {
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var startUtc = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc); // a Monday

        _providers.Setup(p => p.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Provider { Id = providerId, Name = "Alex Provider" });

        _services.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Service { Id = serviceId, Name = "Consultation", DurationMinutes = 30 });

        _providers.Setup(p => p.GetAvailabilityAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Availability>
            {
                new()
                {
                    ProviderId = providerId,
                    DayOfWeek = startUtc.DayOfWeek,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0)
                }
            });

        _appointments.Setup(a => a.HasOverlapAsync(providerId, startUtc, startUtc.AddMinutes(30), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // simulate another booking already occupying this slot

        var sut = BuildService();
        var request = new CreateAppointmentRequest(providerId, serviceId, Guid.NewGuid(), startUtc, null);

        await Assert.ThrowsAsync<BookingConflictException>(() => sut.BookAsync(request));
    }

    [Fact]
    public async Task BookAsync_Throws_WhenRequestedTimeIsOutsideProviderAvailability()
    {
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        // Provider only works 09:00-17:00; request is for 18:00 - after hours.
        var startUtc = new DateTime(2026, 7, 20, 18, 0, 0, DateTimeKind.Utc);

        _providers.Setup(p => p.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Provider { Id = providerId, Name = "Alex Provider" });

        _services.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Service { Id = serviceId, Name = "Consultation", DurationMinutes = 30 });

        _providers.Setup(p => p.GetAvailabilityAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Availability>
            {
                new()
                {
                    ProviderId = providerId,
                    DayOfWeek = startUtc.DayOfWeek,
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(17, 0)
                }
            });

        var sut = BuildService();
        var request = new CreateAppointmentRequest(providerId, serviceId, Guid.NewGuid(), startUtc, null);

        await Assert.ThrowsAsync<OutsideAvailabilityException>(() => sut.BookAsync(request));
    }

    [Fact]
    public async Task GetAvailableSlotsAsync_ReturnsEmptyList_WhenProviderDoesNotWorkThatDayOfWeek()
    {
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var sunday = new DateOnly(2026, 7, 19); // a Sunday, no availability configured

        _services.Setup(s => s.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Service { Id = serviceId, Name = "Consultation", DurationMinutes = 30 });

        _providers.Setup(p => p.GetAvailabilityAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Availability>()); // no Sunday hours at all

        var sut = BuildService();
        var slots = await sut.GetAvailableSlotsAsync(providerId, serviceId, sunday);

        Assert.Empty(slots);
    }
}
