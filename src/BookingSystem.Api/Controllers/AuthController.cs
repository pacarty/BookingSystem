using BookingSystem.Application.DTOs;
using BookingSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Api.Controllers;

// No POST /api/auth/register here on purpose - staff accounts are created
// by an admin (or, in this starter, the DevelopmentSeeder), not
// self-service. Only the public booking flow is open to anonymous use.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthController(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            // Deliberately vague - never reveal whether the email or the
            // password was the wrong part.
            return Unauthorized(new { error = "Invalid email or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var issued = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponse(issued.Token, issued.ExpiresUtc, user.Email!, roles.ToList(), user.ProviderId));
    }
}
