using BookingSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderRepository _providers;

    public ProvidersController(IProviderRepository providers) => _providers = providers;

    // GET /api/providers
    [HttpGet]
    public async Task<IActionResult> GetActiveProviders(CancellationToken ct)
    {
        var providers = await _providers.GetActiveAsync(ct);
        var response = providers.Select(p => new { p.Id, p.Name, p.Bio });
        return Ok(response);
    }

    // GET /api/providers/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var provider = await _providers.GetByIdAsync(id, ct);
        if (provider is null) return NotFound();

        return Ok(new { provider.Id, provider.Name, provider.Bio, provider.Email, provider.Phone });
    }
}
