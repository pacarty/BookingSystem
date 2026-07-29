using System.Security.Claims;
using BookingSystem.Infrastructure.Identity;

namespace BookingSystem.Infrastructure.Identity;

public record IssuedToken(string Token, DateTime ExpiresUtc);

public interface ITokenService
{
    IssuedToken CreateToken(ApplicationUser user, IList<string> roles);
}
