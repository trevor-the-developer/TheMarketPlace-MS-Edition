using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Text;
using AppStatusCodes = AuthenticationService.Application.Constants.StatusCodes;
using AppAuthConstants = AuthenticationService.Application.Constants.AuthConstants;

namespace AuthenticationService.Application.Features.Register.Commands;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ConfirmEmailResponse> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Email confirmation request for email: {Email}", request.Email);

        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogError(AppAuthConstants.UserDoesntExist);
            return new ConfirmEmailResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status401Unauthorized.ToString(),
                    statusCode: StatusCodes.Status401Unauthorized,
                    errorMessage: AppAuthConstants.UserDoesntExist
                )
            };
        }

        // Confirm email
        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            _logger.LogError(AppAuthConstants.ConfirmationFailed);
            return new ConfirmEmailResponse
            {
                ApiError = new ApiError(
                    httpStatusCode: StatusCodes.Status400BadRequest.ToString(),
                    statusCode: StatusCodes.Status400BadRequest,
                    errorMessage: AppAuthConstants.ConfirmationCodeInvalid
                ),
                Errors = result.Errors
            };
        }

        // Generate confirmation code
        var confirmationCode = Guid.NewGuid().ToString();

        _logger.LogInformation(AppAuthConstants.ConfirmationSucceeded);

        return new ConfirmEmailResponse
        {
            Succeeded = true,
            Email = request.Email,
            ConfirmationCode = confirmationCode
        };
    }
}