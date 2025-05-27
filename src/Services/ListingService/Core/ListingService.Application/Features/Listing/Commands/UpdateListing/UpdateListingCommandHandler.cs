using AutoMapper;
using MediatR;
using MassTransit;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Listing.Shared;
using ListingService.Application.Services.CurrentUserService;
using ListingService.Domain.Entities;
using Services.Core.Events.ListingEvents;

namespace ListingService.Application.Features.Listing.Commands.UpdateListing;

public class UpdateListingCommandHandler : IRequestHandler<UpdateListingCommand, ListingDto>
{
    private readonly IListingRepository _listingRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentUserService _currentUserService;

    public UpdateListingCommandHandler(
        IListingRepository listingRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository,
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        ICurrentUserService currentUserService)
    {
        _listingRepository = listingRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _currentUserService = currentUserService;
    }

    public async Task<ListingDto> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User must be authenticated to update listings");
        }

        if (!_currentUserService.IsInRole("Seller"))
        {
            throw new UnauthorizedAccessException("User must have the Seller role to update listings");
        }

        var listing = await _listingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (listing == null)
        {
            return null!;
        }

        // Verify the user owns this listing
        var currentUserId = _currentUserService.NameIdentifier;
        if (currentUserId != null && Guid.TryParse(currentUserId, out Guid userId))
        {
            if (listing.SellerId != userId)
            {
                throw new UnauthorizedAccessException("User can only update their own listings");
            }
        }
        else
        {
            throw new InvalidOperationException("User NameIdentifier could not be determined from authenticated user");
        }

        // Update basic properties
        listing.Title = request.Title;
        listing.Description = request.Description;
        listing.Price = request.Price;
        listing.Location = request.Location;
        listing.CategoryId = request.CategoryId;

        // Update tags
        listing.Tags?.Clear();
        if (request.TagNames.Any())
        {
            listing.Tags = new List<Tag>();
            foreach (var tagName in request.TagNames)
            {
                var tag = await _tagRepository.GetByNameAsync(tagName, cancellationToken);
                if (tag == null)
                {
                    tag = await _tagRepository.AddAsync(new Tag { Name = tagName }, cancellationToken);
                }
                
                listing.Tags.Add(tag);
            }
        }

        // Update images
        listing.Images?.Clear();
        if (request.Images.Any())
        {
            listing.Images = new List<ListingImage>();
            foreach (var imageDto in request.Images)
            {
                var image = new ListingImage
                {
                    Id = imageDto.Id ?? Guid.NewGuid(),
                    ListingId = listing.Id,
                    Url = imageDto.Url,
                    IsMainImage = imageDto.IsMainImage,
                    SortOrder = imageDto.SortOrder
                };
                
                listing.Images.Add(image);
            }
        }

        await _listingRepository.UpdateAsync(listing, cancellationToken);
        var updatedListing = listing;

        // Get category for the event
        var category = await _categoryRepository.GetByIdAsync(updatedListing.CategoryId, cancellationToken);

        // Publish event
        await _publishEndpoint.Publish(new ListingUpdated(
            updatedListing.Id,
            updatedListing.Title,
            updatedListing.Description ?? string.Empty,
            updatedListing.Price,
            updatedListing.Location ?? string.Empty,
            updatedListing.CategoryId,
            category?.Name ?? string.Empty,
            updatedListing.Tags?.Select(t => t.Name).ToArray() ?? Array.Empty<string>(),
            $"/api/listings/{updatedListing.Id}",
            updatedListing.IsActive,
            DateTime.UtcNow
        ), cancellationToken);

        return _mapper.Map<ListingDto>(updatedListing);
    }
}