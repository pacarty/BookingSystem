using BookingSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly IServiceRepository _services;
    private readonly IProviderRepository _providers;

    public ServicesController(IServiceRepository services, IProviderRepository providers)
    {
        _services = services;
        _providers = providers;
    }

    // GET /api/services - the public site's first step: "what do you need booked?"
    [HttpGet]
    public async Task<IActionResult> GetActiveServices(CancellationToken ct)
    {
        var services = await _services.GetActiveAsync(ct);
        var response = services.Select(s => new { s.Id, s.Name, s.Description, s.DurationMinutes, s.Price });
        return Ok(response);
    }

    // GET /api/services/{id}/providers - who can you book this service with?
    [HttpGet("{id:guid}/providers")]
    public async Task<IActionResult> GetProvidersForService(Guid id, CancellationToken ct)
    {
        var service = await _services.GetByIdAsync(id, ct);
        if (service is null) return NotFound();

        var providers = await _providers.GetByServiceIdAsync(id, ct);
        var response = providers.Select(p => new { p.Id, p.Name, p.Bio });
        return Ok(response);
    }
}
