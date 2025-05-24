using AutoMapper;
using MediatR;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto?>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto?> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (category == null)
        {
            return null;
        }
        
        // Update properties
        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;
        
        // Save changes
        await _categoryRepository.UpdateAsync(category, cancellationToken);
        
        // Get parent category name if needed
        if (category.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(category.ParentCategoryId.Value, cancellationToken);
            if (parentCategory != null)
            {
                var result = _mapper.Map<CategoryDto>(category);
                result.ParentCategoryName = parentCategory.Name;
                return result;
            }
        }
        
        return _mapper.Map<CategoryDto>(category);
    }
}