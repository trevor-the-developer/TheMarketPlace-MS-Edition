using AutoMapper;
using MediatR;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Queries.GetSubcategories;

public class GetSubcategoriesQueryHandler : IRequestHandler<GetSubcategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetSubcategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> Handle(GetSubcategoriesQuery request, CancellationToken cancellationToken)
    {
        var subcategories = await _categoryRepository.GetSubcategoriesAsync(request.ParentId, cancellationToken);
        return _mapper.Map<List<CategoryDto>>(subcategories);
    }
}