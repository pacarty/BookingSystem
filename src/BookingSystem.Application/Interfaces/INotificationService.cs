using BookingSystem.Domain.Entities;

namespace BookingSystem.Application.Interfaces;

// One interface, two channels. The starter solution ships a console/log
// implementation for both (see Infrastructure/Notifications). Swapping in
// SendGrid for email or Twilio for SMS later is a matter of adding a new
// class that implements this interface and changing one DI registration
// in Program.cs - nothing in Application or Api needs to change.
public interface INotificationService
{
    Task SendAppointmentConfirmationAsync(Appointment appointment, CancellationToken ct = default);
    Task SendAppointmentReminderAsync(Appointment appointment, CancellationToken ct = default);
}
