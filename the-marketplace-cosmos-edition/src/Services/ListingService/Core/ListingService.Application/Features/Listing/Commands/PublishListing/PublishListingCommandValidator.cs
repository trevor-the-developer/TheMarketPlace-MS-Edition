using FluentValidation;

namespace ListingService.Application.Features.Listing.Commands.PublishListing;

public class PublishListingCommandValidator : AbstractValidator<PublishListingCommand>
{
    public PublishListingCommandValidator()
    {
        RuleFor(p => p.Id).NotEmpty().WithMessage("{PropertyName} is required.");
    }
}