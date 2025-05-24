using AutoMapper;
using MediatR;
using Services.Core.Models;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, BasePagedResult<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<BasePagedResult<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAsync(
            request.PageNumber, 
            request.PageSize, 
            cancellationToken
        );
        
        var categoryDtos = _mapper.Map<List<CategoryDto>>(categories.Data);
        
        return new BasePagedResult<CategoryDto>
        {
            Data = categoryDtos,
            PageNumber = categories.PageNumber,
            PageSize = categories.PageSize,
            TotalRecords = categories.TotalRecords,
            TotalPages = categories.TotalPages
        };
    }
}