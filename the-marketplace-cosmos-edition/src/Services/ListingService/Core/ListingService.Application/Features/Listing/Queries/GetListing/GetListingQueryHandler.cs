using AutoMapper;
using MediatR;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Listing.Shared;

namespace ListingService.Application.Features.Listing.Queries.GetListing;

public class GetListingQueryHandler : IRequestHandler<GetListingQuery, ListingDto?>
{
    private readonly IListingRepository _listingRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetListingQueryHandler(
        IListingRepository listingRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _listingRepository = listingRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<ListingDto?> Handle(GetListingQuery request, CancellationToken cancellationToken)
    {
        var listing = await _listingRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (listing == null)
        {
            return null;
        }
        
        var result = _mapper.Map<ListingDto>(listing);
        
        // Get category info if needed
        if (listing.Category == null && listing.CategoryId != Guid.Empty)
        {
            var category = await _categoryRepository.GetByIdAsync(listing.CategoryId, cancellationToken);
            if (category != null)
            {
                result.CategoryName = category.Name;
            }
        }
        
        return result;
    }
}