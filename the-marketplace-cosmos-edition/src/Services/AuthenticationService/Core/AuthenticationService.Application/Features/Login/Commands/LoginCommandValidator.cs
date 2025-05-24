using FluentValidation;

namespace AuthenticationService.Application.Features.Login.Commands;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .EmailAddress().WithMessage("{PropertyName} is not a valid email address.");

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MinimumLength(6).WithMessage("{PropertyName} must be at least 6 characters.");
    }
}