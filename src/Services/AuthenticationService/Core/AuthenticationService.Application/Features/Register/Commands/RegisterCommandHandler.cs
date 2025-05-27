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
    // Maps Role enum values to the actual role names in the database
    // The IDs in the database are: Admin=1, User=2, Seller=3
    private readonly Dictionary<Role, string> _roleMapping = new()
    {
        { Role.User, "User" },     // Role.User (0) maps to "User" role
        { Role.Seller, "Seller" }, // Role.Seller (1) maps to "Seller" role
        { Role.Admin, "Admin" }    // Role.Admin (2) maps to "Admin" role
    };

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
        if (!_roleMapping.TryGetValue(request.Role, out string roleName))
        {
            roleName = "User"; // Default to User role if the requested role is invalid
            _logger.LogWarning("Invalid role requested: {Role}. Defaulting to User role.", request.Role);
        }

        _logger.LogInformation("Assigning role {RoleName} to user {Email}", roleName, user.Email);
        var roleResult = await _userManager.AddToRoleAsync(user, roleName);
        
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            _logger.LogError("Failed to assign role {Role} to user {Email}. Errors: {Errors}", 
                roleName, user.Email, errors);
            
            // We'll continue with registration but log the error for troubleshooting
            // The user will need an admin to fix their role assignment
        }
        else
        {
            _logger.LogInformation("Successfully assigned role {RoleName} to user {Email}", roleName, user.Email);
        }

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