using Microsoft.AspNetCore.Identity;
using Services.Core.Entities;

namespace AuthenticationService.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}

public enum Role
{
    User = 0,   // Maps to role ID 2 in the database
    Seller = 1, // Maps to role ID 3 in the database
    Admin = 2   // Maps to role ID 1 in the database
}
