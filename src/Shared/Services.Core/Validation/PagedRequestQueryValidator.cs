using FluentValidation;
using Services.Core.Models;

namespace Services.Core.Validation
{
    public abstract class PagedRequestQueryValidator<T> : AbstractValidator<T> where T : PagedRequestQuery
    {
        protected PagedRequestQueryValidator()
        {
            RuleFor(p => p.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("{PropertyName} must be greater than or equal to 1.");

            RuleFor(p => p.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("{PropertyName} must be greater than or equal to 1.");
        }
    }
}