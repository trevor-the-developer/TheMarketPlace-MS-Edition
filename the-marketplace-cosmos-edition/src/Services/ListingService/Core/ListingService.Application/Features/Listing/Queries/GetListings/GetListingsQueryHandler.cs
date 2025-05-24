using AutoMapper;
using MediatR;
using Services.Core.Models;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Listing.Shared;

namespace ListingService.Application.Features.Listing.Queries.GetListings;

public class GetListingsQueryHandler : IRequestHandler<GetListingsQuery, BasePagedResult<ListingDto>>
{
    private readonly IListingRepository _listingRepository;
    private readonly IMapper _mapper;

    public GetListingsQueryHandler(IListingRepository listingRepository, IMapper mapper)
    {
        _listingRepository = listingRepository;
        _mapper = mapper;
    }

    public async Task<BasePagedResult<ListingDto>> Handle(GetListingsQuery request, CancellationToken cancellationToken)
    {
        BasePagedResult<Domain.Entities.Listing> listings;
        
        if (request.CategoryId.HasValue)
        {
            listings = await _listingRepository.GetByCategoryIdAsync(
                request.CategoryId.Value, 
                request.PageNumber, 
                request.PageSize, 
                cancellationToken
            );
        }
        else if (request.SellerId.HasValue)
        {
            listings = await _listingRepository.GetBySellerIdAsync(
                request.SellerId.Value,
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );
        }
        else
        {
            listings = await _listingRepository.GetAsync(
                request.PageNumber,
                request.PageSize,
                cancellationToken
            );
        }
        
        // Map to DTO
        var listingDtos = _mapper.Map<List<ListingDto>>(listings.Data);
        
        return new BasePagedResult<ListingDto>
        {
            Data = listingDtos,
            PageNumber = listings.PageNumber,
            PageSize = listings.PageSize,
            TotalRecords = listings.TotalRecords,
            TotalPages = listings.TotalPages
        };
    }
}