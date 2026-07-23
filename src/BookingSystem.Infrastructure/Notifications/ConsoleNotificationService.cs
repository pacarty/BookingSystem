using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BookingSystem.Infrastructure.Notifications;

// Default implementation: logs what WOULD be sent instead of calling a paid
// provider. This lets the whole booking flow - including the notification
// step - work end to end with zero cost and zero external accounts.
//
// To wire up real email, add a SendGridNotificationService (or similar)
// implementing INotificationService and swap the registration in
// Program.cs. To add SMS, add an ISmsSender abstraction underneath this
// same interface and implement it with Twilio when you're ready to spend
// a few dollars on a demo.
public class ConsoleNotificationService : INotificationService
{
    private readonly ILogger<ConsoleNotificationService> _logger;

    public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger) => _logger = logger;

    public Task SendAppointmentConfirmationAsync(Appointment appointment, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[Notification] Confirmation for appointment {AppointmentId}: {Start:u} - {End:u}",
            appointment.Id, appointment.StartUtc, appointment.EndUtc);
        return Task.CompletedTask;
    }

    public Task SendAppointmentReminderAsync(Appointment appointment, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[Notification] Reminder for appointment {AppointmentId}: {Start:u}",
            appointment.Id, appointment.StartUtc);
        return Task.CompletedTask;
    }
}
