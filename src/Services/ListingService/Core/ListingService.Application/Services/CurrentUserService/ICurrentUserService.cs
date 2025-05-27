using System.Security.Claims;

namespace ListingService.Application.Services.CurrentUserService;

public interface ICurrentUserService
{
    string? NameIdentifier { get; }
    string? Username { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    IEnumerable<Claim> GetClaims();
    IEnumerable<string> GetRoles();
}