using AutoMapper;
using MediatR;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Category.Shared;
using ListingService.Domain.Entities;

namespace ListingService.Application.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Domain.Entities.Category
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId
        };
        
        var createdCategory = await _categoryRepository.AddAsync(category, cancellationToken);
        
        // If this is a subcategory, get the parent name
        if (createdCategory.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(createdCategory.ParentCategoryId.Value, cancellationToken);
            if (parentCategory != null)
            {
                var result = _mapper.Map<CategoryDto>(createdCategory);
                result.ParentCategoryName = parentCategory.Name;
                return result;
            }
        }
        
        return _mapper.Map<CategoryDto>(createdCategory);
    }
}