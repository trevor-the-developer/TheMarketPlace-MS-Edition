using AuthenticationService.Application.Features.Login.Commands;
using AuthenticationService.Application.Features.Register.Commands;
using AuthenticationService.Application.Features.Token.Commands;
using AuthenticationService.Application.Settings;
using AuthenticationService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core.Models.Service;

namespace AuthenticationService.Api.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var response = await _mediator.Send(command);

        if (response.ApiError != null)
        {
            return StatusCode(response.ApiError.StatusCode, ServiceResponse<LoginResponse>.Error(
                response.ApiError.ErrorMessage));
        }

        if (response.Succeeded.HasValue && response.Succeeded.Value)
        {
            return Ok(ServiceResponse<LoginResponse>.Success(response));
        }

        return UnprocessableEntity(ServiceResponse<LoginResponse>.Error("Login failed"));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponse<RegisterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var response = await _mediator.Send(command);

        if (response.ApiError != null)
        {
            return StatusCode(response.ApiError.StatusCode, ServiceResponse<RegisterResponse>.Error(
                response.ApiError.ErrorMessage));
        }

        if (response.RegistrationStepOne.HasValue && response.RegistrationStepOne.Value)
        {
            return Ok(ServiceResponse<RegisterResponse>.Success(response));
        }

        if (response.Errors != null && response.Errors.Any())
        {
            return BadRequest(ServiceResponse<RegisterResponse>.Error(
                string.Join(", ", response.Errors.Select(e => e.Description))));
        }

        return UnprocessableEntity(ServiceResponse<RegisterResponse>.Error("Registration failed"));
    }

    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponse<ConfirmEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        var command = new ConfirmEmailCommand { Email = email, Token = token };
        var response = await _mediator.Send(command);

        if (response.ApiError != null)
        {
            return StatusCode(response.ApiError.StatusCode, ServiceResponse<ConfirmEmailResponse>.Error(
                response.ApiError.ErrorMessage));
        }

        if (response.Succeeded.HasValue && response.Succeeded.Value)
        {
            return Ok(ServiceResponse<ConfirmEmailResponse>.Success(response));
        }

        if (response.Errors != null && response.Errors.Any())
        {
            return BadRequest(ServiceResponse<ConfirmEmailResponse>.Error(
                string.Join(", ", response.Errors.Select(e => e.Description))));
        }

        return UnprocessableEntity(ServiceResponse<ConfirmEmailResponse>.Error("Email confirmation failed"));
    }

    [HttpPost("refresh")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var response = await _mediator.Send(command);

        if (response.ApiError != null)
        {
            if (response.ApiError.StatusCode == StatusCodes.Status401Unauthorized)
            {
                return Unauthorized(ServiceResponse<TokenResponse>.Error(
                    response.ApiError.ErrorMessage));
            }

            return StatusCode(response.ApiError.StatusCode, ServiceResponse<TokenResponse>.Error(
                response.ApiError.ErrorMessage));
        }

        if (response.Succeeded.HasValue && response.Succeeded.Value)
        {
            return Ok(ServiceResponse<TokenResponse>.Success(response));
        }

        return UnprocessableEntity(ServiceResponse<TokenResponse>.Error("Token refresh failed"));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ServiceResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenCommand command)
    {
        var response = await _mediator.Send(command);

        if (response.ApiError != null)
        {
            return StatusCode(response.ApiError.StatusCode, ServiceResponse<TokenResponse>.Error(
                response.ApiError.ErrorMessage));
        }

        if (response.Succeeded.HasValue && response.Succeeded.Value)
        {
            return Ok(ServiceResponse<TokenResponse>.Success(response));
        }

        return UnprocessableEntity(ServiceResponse<TokenResponse>.Error("Logout failed"));
    }
}