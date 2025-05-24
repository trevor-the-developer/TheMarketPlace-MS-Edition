using AutoMapper;
using MediatR;
using MassTransit;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Listing.Shared;
using ListingService.Domain.Entities;
using Services.Core.Events.ListingEvents;

namespace ListingService.Application.Features.Listing.Commands.PublishListing;

public class PublishListingCommandHandler : IRequestHandler<PublishListingCommand, ListingDto?>
{
    private readonly IListingRepository _listingRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public PublishListingCommandHandler(
        IListingRepository listingRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _listingRepository = listingRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ListingDto?> Handle(PublishListingCommand request, CancellationToken cancellationToken)
    {
        var listing = await _listingRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (listing == null)
        {
            return null;
        }
        
        // Update listing status
        listing.Status = ListingStatus.Published;
        listing.PublishedAt = DateTime.UtcNow;
        
        await _listingRepository.UpdateAsync(listing, cancellationToken);
        
        // Get category for the event
        var category = await _categoryRepository.GetByIdAsync(listing.CategoryId, cancellationToken);
        
        // Publish event
        await _publishEndpoint.Publish(new ListingPublished(
            listing.Id,
            listing.PublishedAt ?? DateTime.UtcNow
        ), cancellationToken);
        
        // Return mapped DTO
        return _mapper.Map<ListingDto>(listing);
    }
}