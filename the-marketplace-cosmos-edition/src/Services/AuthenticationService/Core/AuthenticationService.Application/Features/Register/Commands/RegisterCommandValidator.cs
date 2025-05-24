using FluentValidation;

namespace AuthenticationService.Application.Features.Register.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.");

        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.");

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .EmailAddress().WithMessage("{PropertyName} is not a valid email address.");

        RuleFor(p => p.Password)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MinimumLength(6).WithMessage("{PropertyName} must be at least 6 characters.");

        RuleFor(p => p.DateOfBirth)
            .Must(date => date == null || date < DateTime.Now.AddYears(-13))
            .WithMessage("You must be at least 13 years old.");
    }
}