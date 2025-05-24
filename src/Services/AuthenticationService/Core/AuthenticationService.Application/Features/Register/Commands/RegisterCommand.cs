using AuthenticationService.Domain.Entities;
using MediatR;

namespace AuthenticationService.Application.Features.Register.Commands;

public class RegisterCommand : IRequest<RegisterResponse>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Role Role { get; set; } = Role.User;
}

public class RegisterResponse
{
    public bool? RegistrationStepOne { get; set; }
    public string? Email { get; set; }
    public string? ConfirmationToken { get; set; }
    public ApiError? ApiError { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Identity.IdentityError>? Errors { get; set; }
}