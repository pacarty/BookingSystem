using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    // A trivial, unauthenticated endpoint - useful on its own for uptime
    // monitoring, and handy right now as a quick way to prove a deploy
    // actually landed without needing to touch the database.
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", timestampUtc = DateTime.UtcNow });
}