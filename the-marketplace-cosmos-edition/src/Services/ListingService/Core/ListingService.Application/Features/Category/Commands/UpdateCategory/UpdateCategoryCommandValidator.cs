using FluentValidation;
using ListingService.Application.Contracts.Persistence;

namespace ListingService.Application.Features.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;

        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("{PropertyName} is required.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.");

        RuleFor(p => p.ParentCategoryId)
            .MustAsync(CategoryMustExistWhenSpecified).WithMessage("Parent category with the specified ID does not exist.")
            .Must((command, parentId) => parentId != command.Id).WithMessage("Category cannot be its own parent.");
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