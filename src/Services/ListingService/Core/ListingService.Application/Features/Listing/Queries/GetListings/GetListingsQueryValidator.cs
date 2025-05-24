using FluentValidation;
using Services.Core.Validation;

namespace ListingService.Application.Features.Listing.Queries.GetListings;

public class GetListingsQueryValidator : PagedRequestQueryValidator<GetListingsQuery>
{
    public GetListingsQueryValidator()
    {
        RuleFor(p => p.MinPrice)
            .GreaterThanOrEqualTo(0).When(p => p.MinPrice.HasValue)
            .WithMessage("{PropertyName} must be greater than or equal to 0.");
            
        RuleFor(p => p.MaxPrice)
            .GreaterThanOrEqualTo(p => p.MinPrice ?? 0).When(p => p.MaxPrice.HasValue)
            .WithMessage("{PropertyName} must be greater than or equal to MinPrice.");
    }
}