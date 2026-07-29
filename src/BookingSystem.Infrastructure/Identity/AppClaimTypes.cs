namespace BookingSystem.Infrastructure.Identity;

public static class AppClaimTypes
{
    // Custom claim carrying the Provider.Id for Provider-role accounts, so
    // controllers can do resource-based checks ("is this your appointment?")
    // without an extra database round trip.
    public const string ProviderId = "providerId";
}
