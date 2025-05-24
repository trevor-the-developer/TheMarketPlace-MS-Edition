using AuthenticationService.Domain.Entities;
using MediatR;

namespace AuthenticationService.Application.Features.Register.Commands;

public class ConfirmEmailCommand : IRequest<ConfirmEmailResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ConfirmEmailResponse
{
    public bool? Succeeded { get; set; }
    public string? Email { get; set; }
    public string? ConfirmationCode { get; set; }
    public ApiError? ApiError { get; set; }
    public IEnumerable<Microsoft.AspNetCore.Identity.IdentityError>? Errors { get; set; }
}