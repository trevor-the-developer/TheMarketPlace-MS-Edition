using FluentValidation;
using ListingService.Application.Contracts.Persistence;

namespace ListingService.Application.Features.Listing.Commands.CreateListing;

public class CreateListingCommandValidator : AbstractValidator<CreateListingCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateListingCommandValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(2000).WithMessage("{PropertyName} must not exceed 2000 characters.");

        RuleFor(p => p.Price)
            .GreaterThanOrEqualTo(0).WithMessage("{PropertyName} must be greater than or equal to 0.");

        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MustAsync(CategoryMustExist).WithMessage("Category with the specified ID does not exist.");

        RuleFor(p => p.SellerId)
            .NotEmpty().WithMessage("{PropertyName} is required.");

        RuleForEach(p => p.TagNames)
            .NotEmpty().WithMessage("Tag names cannot be empty.")
            .MaximumLength(50).WithMessage("Tag names must not exceed 50 characters.");

        RuleForEach(p => p.Images)
            .SetValidator(new CreateListingImageCommandValidator());
    }

    private async Task<bool> CategoryMustExist(Guid categoryId, CancellationToken token)
    {
        return await _categoryRepository.ExistsByIdAsync(categoryId, token);
    }
}

public class CreateListingImageCommandValidator : AbstractValidator<CreateListingImageCommand>
{
    public CreateListingImageCommandValidator()
    {
        RuleFor(p => p.Url)
            .NotEmpty().WithMessage("Image URL is required.")
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters.");
    }
}