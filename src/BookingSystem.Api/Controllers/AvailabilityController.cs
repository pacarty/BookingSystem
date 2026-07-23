using BookingSystem.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvailabilityController : ControllerBase
{
    private readonly IAppointmentBookingService _bookingService;

    public AvailabilityController(IAppointmentBookingService bookingService) => _bookingService = bookingService;

    // GET /api/availability?providerId=...&serviceId=...&date=2026-07-20
    // Returns the bookable slots for a single day - the public site calls
    // this once per date the client picks in the calendar UI.
    [HttpGet]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid providerId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date, CancellationToken ct)
    {
        try
        {
            var slots = await _bookingService.GetAvailableSlotsAsync(providerId, serviceId, date, ct);
            return Ok(slots);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
