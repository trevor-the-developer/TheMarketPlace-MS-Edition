using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ListingService.Application.Services.CurrentUserService;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? NameIdentifier
    {
        get
        {
            // Get all NameIdentifier claims and find the one that looks like a GUID (not an email)
            var claims = _httpContextAccessor.HttpContext?.User?.Claims?
                .Where(c => c.Type == ClaimTypes.NameIdentifier || 
                           c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                .ToList();

            if (claims != null && claims.Any())
            {
                // Look for a GUID format claim first
                var guidClaim = claims.FirstOrDefault(c => Guid.TryParse(c.Value, out _));
                if (guidClaim != null)
                {
                    return guidClaim.Value;
                }
                
                // Fallback to first claim if no GUID found
                return claims.First().Value;
            }

            // Additional fallbacks
            return _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value ??
                   _httpContextAccessor.HttpContext?.User?.FindFirst("nameid")?.Value;
        }
    }
    
    public string? Username => 
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ??
        _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
    
    public string? Email => 
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ??
        _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;
    
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    
    public IEnumerable<Claim> GetClaims() => _httpContextAccessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
    
    public IEnumerable<string> GetRoles() => 
        _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value) ?? Enumerable.Empty<string>();
}