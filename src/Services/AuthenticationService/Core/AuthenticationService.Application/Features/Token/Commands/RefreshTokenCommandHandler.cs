using System.IdentityModel.Tokens.Jwt;
using AuthenticationService.Application.Contracts.Security;
using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AppStatusCodes = AuthenticationService.Application.Constants.StatusCodes;
using AppAuthConstants = AuthenticationService.Application.Constants.AuthConstants;

namespace AuthenticationService.Application.Features.Token.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token refresh request");

        var secret = _configuration[AppAuthConstants.JwtSettingsKey] ??
                      throw new InvalidOperationException(AppAuthConstants.SecretKeyNotConfigured);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = true,
            ValidIssuer = _configuration[AppAuthConstants.JwtSettingsIssuer],
            ValidateAudience = true,
            ValidAudience = _configuration[AppAuthConstants.JwtSettingsAudience],
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        // Get principal from expired token
        var principal = _tokenService.GetPrincipalFromExpiredToken(
            request.Token, 
            tokenValidationParameters, 
            _configuration);

        if (principal == null)
        {
            _logger.LogError(AppAuthConstants.InvalidToken);
            return new TokenResponse
            {
                Succeeded = false,
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status401Unauthorized.ToString(),
                    statusCode: StatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.InvalidToken
                )
            };
        }

        // Get user from principal
        var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogError(AppAuthConstants.InvalidToken);
            return new TokenResponse
            {
                Succeeded = false,
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status401Unauthorized.ToString(),
                    statusCode: StatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.InvalidToken
                )
            };
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.RefreshToken != request.RefreshToken)
        {
            _logger.LogError(AppAuthConstants.InvalidRefreshToken);
            return new TokenResponse
            {
                Succeeded = false,
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status401Unauthorized.ToString(),
                    statusCode: StatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.InvalidRefreshToken
                )
            };
        }

        if (user.RefreshTokenExpiry <= DateTime.UtcNow)
        {
            _logger.LogError(AppAuthConstants.RefreshTokenExpired);
            return new TokenResponse
            {
                Succeeded = false,
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status401Unauthorized.ToString(),
                    statusCode: StatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.RefreshTokenExpired
                )
            };
        }

        // Generate new JWT token
        var token = await _tokenService.GenerateJwtSecurityTokenAsync(_userManager, user, _configuration);
        
        // Generate new refresh token
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(30);

        // Update user with new refresh token
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError(AppAuthConstants.TokenRefreshFailed);
            return new TokenResponse
            {
                Succeeded = false,
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status500InternalServerError.ToString(),
                    statusCode: StatusCodes.Status500InternalServerError,
                    errorMessage: AppAuthConstants.TokenRefreshFailed
                ),
                Errors = result.Errors
            };
        }

        _logger.LogInformation(AppAuthConstants.TokenRefreshSucceeded);

        return new TokenResponse
        {
            Succeeded = true,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = token.ValidTo,
            RefreshToken = refreshToken
        };
    }
}