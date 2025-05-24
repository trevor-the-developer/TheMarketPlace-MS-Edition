using AuthenticationService.Domain.Entities;
using MediatR;

namespace AuthenticationService.Application.Features.Token.Commands;

public class RefreshTokenCommand : IRequest<TokenResponse>
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}