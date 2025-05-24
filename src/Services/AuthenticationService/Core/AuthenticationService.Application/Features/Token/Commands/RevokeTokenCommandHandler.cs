using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using AppStatusCodes = AuthenticationService.Application.Constants.StatusCodes;
using AppAuthConstants = AuthenticationService.Application.Constants.AuthConstants;

namespace AuthenticationService.Application.Features.Token.Commands;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, TokenResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RevokeTokenCommandHandler> _logger;

    public RevokeTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<RevokeTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<TokenResponse> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Token revoke request for email: {Email}", request.Email);

        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogError(AppAuthConstants.UserDoesntExist);
            return new TokenResponse
            {
                Succeeded = false,
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status401Unauthorized.ToString(),
                    statusCode: StatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.UserDoesntExist
                )
            };
        }

        // Revoke refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        // Update user
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError(AppAuthConstants.LogoutFailed);
            return new TokenResponse
            {
                Succeeded = false,
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status500InternalServerError.ToString(),
                    statusCode: StatusCodes.Status500InternalServerError,
                    errorMessage: AppAuthConstants.LogoutFailed
                ),
                Errors = result.Errors
            };
        }

        _logger.LogInformation(AppAuthConstants.LogoutSucceeded);

        return new TokenResponse
        {
            Succeeded = true
        };
    }
}