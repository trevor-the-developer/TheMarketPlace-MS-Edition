using FluentValidation;

namespace ListingService.Application.Features.Listing.Queries.GetListing;

public class GetListingQueryValidator : AbstractValidator<GetListingQuery>
{
    public GetListingQueryValidator()
    {
        RuleFor(p => p.Id).NotEmpty().WithMessage("{PropertyName} is required.");
    }
}