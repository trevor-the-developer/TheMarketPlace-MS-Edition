using AuthenticationService.Domain.Entities;
using MediatR;

namespace AuthenticationService.Application.Features.Token.Commands;

public class RevokeTokenCommand : IRequest<TokenResponse>
{
    public string Email { get; set; } = string.Empty;
}