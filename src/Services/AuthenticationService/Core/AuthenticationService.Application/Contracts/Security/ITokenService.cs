using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthenticationService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Application.Contracts.Security;

public interface ITokenService
{
    Task<JwtSecurityToken> GenerateJwtSecurityTokenAsync(UserManager<ApplicationUser> userManager, ApplicationUser applicationUser, IConfiguration configuration);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, TokenValidationParameters tokenValidationParameters, IConfiguration configuration);
}