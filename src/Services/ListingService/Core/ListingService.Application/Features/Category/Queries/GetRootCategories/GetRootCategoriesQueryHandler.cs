using AutoMapper;
using MediatR;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Queries.GetRootCategories;

public class GetRootCategoriesQueryHandler : IRequestHandler<GetRootCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetRootCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> Handle(GetRootCategoriesQuery request, CancellationToken cancellationToken)
    {
        var rootCategories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);
        return _mapper.Map<List<CategoryDto>>(rootCategories);
    }
}