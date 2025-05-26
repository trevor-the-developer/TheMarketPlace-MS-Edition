using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using AppStatusCodes = AuthenticationService.Application.Constants.StatusCodes;
using AppAuthConstants = AuthenticationService.Application.Constants.AuthConstants;

namespace AuthenticationService.Application.Features.Register.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registration request for email: {Email}", request.Email);

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogError(AppAuthConstants.EmailAlreadyExists);
            return new RegisterResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status409Conflict.ToString(),
                    statusCode: StatusCodes.Status409Conflict,
                    errorMessage: AppAuthConstants.EmailAlreadyExists
                )
            };
        }

        // Create new user
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            _logger.LogError(AppAuthConstants.RegistrationFailed);
            return new RegisterResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status400BadRequest.ToString(),
                    statusCode: StatusCodes.Status400BadRequest,
                    errorMessage: AppAuthConstants.RegistrationFailed
                ),
                Errors = result.Errors
            };
        }

        // Add role to user
        await _userManager.AddToRoleAsync(user, request.Role.ToString());

        // Generate email confirmation token
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        _logger.LogInformation(AppAuthConstants.RegistrationSucceeded);
        
        return new RegisterResponse
        {
            RegistrationStepOne = true,
            Email = request.Email,
            ConfirmationToken = token
        };
    }
}