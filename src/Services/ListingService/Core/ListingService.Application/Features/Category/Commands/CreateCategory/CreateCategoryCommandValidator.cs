using FluentValidation;
using ListingService.Application.Contracts.Persistence;

namespace ListingService.Application.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.");

        RuleFor(p => p.ParentCategoryId)
            .MustAsync(CategoryMustExistWhenSpecified).WithMessage("Parent category with the specified ID does not exist.");
    }

    private async Task<bool> CategoryMustExistWhenSpecified(Guid? parentCategoryId, CancellationToken token)
    {
        if (parentCategoryId == null || parentCategoryId == Guid.Empty)
        {
            return true;
        }
        
        return await _categoryRepository.ExistsByIdAsync(parentCategoryId.Value, token);
    }
}