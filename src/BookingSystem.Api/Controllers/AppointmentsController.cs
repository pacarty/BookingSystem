using System.Security.Claims;
using BookingSystem.Application.DTOs;
using BookingSystem.Application.Exceptions;
using BookingSystem.Application.Interfaces;
using BookingSystem.Application.Services;
using BookingSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
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

    // POST /api/appointments - called by the public booking site. Anonymous
    // on purpose: clients never authenticate (see AuthController).
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

    // GET /api/appointments - the Admin dashboard's all-providers schedule view.
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var appointments = await _appointments.GetAllBetweenAsync(
            DateTime.UtcNow, DateTime.UtcNow.AddDays(30), ct);

        var response = appointments.Select(a => new AppointmentResponse(
            a.Id, a.ProviderId, a.Provider.Name, a.ClientId,
            $"{a.Client.FirstName} {a.Client.LastName}", a.ServiceId, a.Service.Name,
            a.StartUtc, a.EndUtc, a.Status, a.Notes));

        return Ok(response);
    }

    // GET /api/appointments/mine - the provider admin site's schedule view.
    // Scoped to the calling provider via the "providerId" claim on their
    // JWT - a Provider can never pass a different provider's ID to see
    // someone else's bookings, because there's no ID parameter to tamper
    // with in the first place.
    [Authorize(Roles = "Provider")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var providerId = GetCallerProviderId();
        if (providerId is null) return Forbid();

        // A real admin UI would take date-range/paging query params; this
        // starter shows the next 30 days as a reasonable default.
        var appointments = await _appointments.GetForProviderBetweenAsync(
            providerId.Value, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), ct);

        var response = appointments.Select(a => new AppointmentResponse(
            a.Id, a.ProviderId, a.Provider.Name, a.ClientId,
            $"{a.Client.FirstName} {a.Client.LastName}", a.ServiceId, a.Service.Name,
            a.StartUtc, a.EndUtc, a.Status, a.Notes));

        return Ok(response);
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
    // to mark Attended / NoShow / Cancelled. Admins can update any
    // appointment; a Provider can only update their own - role membership
    // alone isn't enough authorization here, ownership matters too.
    [Authorize(Roles = "Admin,Provider")]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
    {
        var appointment = await _appointments.GetByIdAsync(id, ct);
        if (appointment is null) return NotFound();

        if (User.IsInRole("Provider") && !User.IsInRole("Admin"))
        {
            var providerId = GetCallerProviderId();
            if (providerId != appointment.ProviderId) return Forbid();
        }

        appointment.Status = request.Status;
        appointment.UpdatedUtc = DateTime.UtcNow;
        // EF Core is already tracking this entity because it was loaded via
        // GetByIdAsync in the same DbContext scope, so no explicit Update()
        // call is needed - just save.
        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    private Guid? GetCallerProviderId()
    {
        var claim = User.FindFirst(AppClaimTypes.ProviderId)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
