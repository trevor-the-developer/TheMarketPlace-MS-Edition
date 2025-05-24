using Microsoft.AspNetCore.Identity;
using Services.Core.Entities;

namespace AuthenticationService.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool? EmailConfirmed { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}

public enum Role
{
    Admin,
    User,
    Seller
}