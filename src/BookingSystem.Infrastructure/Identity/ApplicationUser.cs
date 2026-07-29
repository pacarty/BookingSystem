using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BookingSystem.Infrastructure.Identity;

// A login account. Clients never get one of these - only staff (Providers
// and Admins) authenticate, matching how the original system worked.
// ProviderId is null for Admin accounts and set for Provider accounts,
// linking the login to the domain's Provider entity so the API can answer
// "which provider is this?" from a JWT claim without an extra lookup table.
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? ProviderId { get; set; }
    public Provider? Provider { get; set; }
}
