namespace BookingSystem.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    DateTime ExpiresUtc,
    string Email,
    IReadOnlyList<string> Roles,
    Guid? ProviderId
);
