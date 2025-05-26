using System.IdentityModel.Tokens.Jwt;
using AuthenticationService.Application.Constants;
using AuthenticationService.Application.Contracts.Security;
using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AppAuthConstants = AuthenticationService.Application.Constants.AuthConstants;
using AppStatusCodes = AuthenticationService.Application.Constants.StatusCodes;

namespace AuthenticationService.Application.Features.Login.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login request for email: {Email}", request.Email);

        // Find the user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogError(AppAuthConstants.UserDoesntExist);
            return new LoginResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: AppStatusCodes.Status401Unauthorized.ToString(),
                    statusCode: AppStatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.UserDoesntExist
                )
            };
        }

        // Check password
        var result = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!result)
        {
            _logger.LogError(AppAuthConstants.InvalidEmailPassword);
            return new LoginResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: AppStatusCodes.Status401Unauthorized.ToString(),
                    statusCode: AppStatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.InvalidEmailPassword
                )
            };
        }

        // Check if email is confirmed
        if (!user.EmailConfirmed)
        {
            _logger.LogError(AppAuthConstants.UserEmailNotConfirmed);
            return new LoginResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: AppStatusCodes.Status401Unauthorized.ToString(),
                    statusCode: AppStatusCodes.EmailNotConfirmed,
                    errorMessage: AppAuthConstants.UserEmailNotConfirmed
                )
            };
        }

        // Generate token
        var token = await _tokenService.GenerateJwtSecurityTokenAsync(_userManager, user, _configuration);
        if (token == null)
        {
            _logger.LogError(AppAuthConstants.LoginFailed);
            return new LoginResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: AppStatusCodes.Status500InternalServerError.ToString(),
                    statusCode: AppStatusCodes.Status500InternalServerError,
                    errorMessage: AppAuthConstants.LoginFailed
                )
            };
        }

        // Generate refresh token
        var refreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(30);

        // Update user with refresh token
        var loginResponse = new LoginResponse();
        var updateResult = await _userManager.UpdateAsync(user);
        
        if (updateResult.Succeeded)
        {
            _logger.LogInformation(AppAuthConstants.LoginSucceeded);
            loginResponse.Succeeded = true;
            loginResponse.Id = user.Id;
            loginResponse.Email = request.Email;
            loginResponse.RefreshToken = refreshToken;
            loginResponse.SecurityToken = new JwtSecurityTokenHandler().WriteToken(token);
            loginResponse.Expiration = token.ValidTo;
        }
        else
        {
            _logger.LogError(AppAuthConstants.LoginFailed);
            loginResponse.ApiError = new ApiError(
                httpStatusCode: AppStatusCodes.Status500InternalServerError.ToString(),
                statusCode: AppStatusCodes.Status500InternalServerError,
                errorMessage: AppAuthConstants.LoginFailed,
                stackTrace: string.Join(", ", updateResult.Errors.Select(e => e.Description))
            );
            loginResponse.Errors = updateResult.Errors;
        }

        return loginResponse;
    }
}