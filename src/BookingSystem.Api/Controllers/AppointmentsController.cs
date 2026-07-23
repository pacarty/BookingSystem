using BookingSystem.Application.DTOs;
using BookingSystem.Application.Exceptions;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentBookingService _bookingService;
    private readonly IAppointmentRepository _appointments;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentsController(
        IAppointmentBookingService bookingService, IAppointmentRepository appointments, IUnitOfWork unitOfWork)
    {
        _bookingService = bookingService;
        _appointments = appointments;
        _unitOfWork = unitOfWork;
    }

    // POST /api/appointments - called by the public booking site
    [HttpPost]
    public async Task<IActionResult> Book([FromBody] CreateAppointmentRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _bookingService.BookAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (OutsideAvailabilityException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (BookingConflictException ex)
        {
            // 409 is the correct status for "someone else booked this slot first" -
            // the client should re-fetch availability and let the user pick again.
            return Conflict(new { error = ex.Message });
        }
    }

    // GET /api/appointments/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var appointment = await _appointments.GetByIdAsync(id, ct);
        if (appointment is null) return NotFound();

        var response = new AppointmentResponse(
            appointment.Id,
            appointment.ProviderId,
            appointment.Provider.Name,
            appointment.ClientId,
            $"{appointment.Client.FirstName} {appointment.Client.LastName}",
            appointment.ServiceId,
            appointment.Service.Name,
            appointment.StartUtc,
            appointment.EndUtc,
            appointment.Status,
            appointment.Notes);

        return Ok(response);
    }

    // PATCH /api/appointments/{id}/status - called by the provider admin site
    // to mark Attended / NoShow / Cancelled.
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await _appointments.GetByIdAsync(id, ct);
        if (appointment is null) return NotFound();

        appointment.Status = request.Status;
        appointment.UpdatedUtc = DateTime.UtcNow;
        // EF Core is already tracking this entity because it was loaded via
        // GetByIdAsync in the same DbContext scope, so no explicit Update()
        // call is needed - just save.
        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
