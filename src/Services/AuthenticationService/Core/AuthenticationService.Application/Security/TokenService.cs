using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthenticationService.Application.Constants;
using AuthenticationService.Application.Contracts.Security;
using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Application.Security;

public class TokenService : ITokenService
{
    public async Task<JwtSecurityToken> GenerateJwtSecurityTokenAsync(UserManager<ApplicationUser> userManager, 
        ApplicationUser applicationUser, IConfiguration configuration)
    {
        var secret = configuration[AuthConstants.JwtSettingsKey] ?? 
                     throw new InvalidOperationException(AuthConstants.SecretKeyNotConfigured);
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(applicationUser);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
        var userClaims = await userManager.GetClaimsAsync(applicationUser);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Name, applicationUser.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Sub, applicationUser.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, applicationUser.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, applicationUser.Id),
        }.Union(userClaims).Union(roleClaims);

        var expiresInMinutes = int.TryParse(configuration[AuthConstants.JwtSettingsExpires], out var minutes) 
            ? minutes 
            : 60; // Default to 60 minutes if not specified

        var token = new JwtSecurityToken(
            issuer: configuration[AuthConstants.JwtSettingsIssuer],
            audience: configuration[AuthConstants.JwtSettingsAudience],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
            );

        return token;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, 
        TokenValidationParameters tokenValidationParameters, IConfiguration configuration)
    {
        var secret = configuration[AuthConstants.JwtSettingsKey] ??
                     throw new InvalidOperationException(AuthConstants.SecretKeyNotConfigured);

        tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = configuration[AuthConstants.JwtSettingsIssuer],
            ValidateAudience = true,
            ValidAudience = configuration[AuthConstants.JwtSettingsAudience],
            ValidateLifetime = false,
            NameClaimType = "sub",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.Zero
        };

        ClaimsPrincipal? validationResult;
        
        try
        {
            validationResult = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out _);
        }
        catch (Exception)
        {
            return null;
        }
        return validationResult;
    }
}