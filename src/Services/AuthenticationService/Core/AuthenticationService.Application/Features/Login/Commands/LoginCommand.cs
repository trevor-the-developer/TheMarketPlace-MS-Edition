using AuthenticationService.Domain.Entities;
using MediatR;

namespace AuthenticationService.Application.Features.Login.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool? Succeeded { get; set; }
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? SecurityToken { get; set; }
    public DateTime? Expiration { get; set; }
    public string? RefreshToken { get; set; }
    public ApiError? ApiError { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Identity.IdentityError>? Errors { get; set; }
}