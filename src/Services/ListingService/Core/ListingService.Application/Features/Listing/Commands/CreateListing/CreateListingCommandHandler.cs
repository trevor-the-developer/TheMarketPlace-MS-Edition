using AutoMapper;
using MediatR;
using MassTransit;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Features.Listing.Shared;
using ListingService.Application.Services.CurrentUserService;
using ListingService.Domain.Entities;
using Services.Core.Events.ListingEvents;
using System;

namespace ListingService.Application.Features.Listing.Commands.CreateListing;

public class CreateListingCommandHandler : IRequestHandler<CreateListingCommand, ListingDto>
{
    private readonly IListingRepository _listingRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentUserService _currentUserService;

    public CreateListingCommandHandler(
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

    public async Task<ListingDto> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create listings");
        }

        if (!_currentUserService.IsInRole("Seller"))
        {
            throw new UnauthorizedAccessException("User must have the Seller role to create listings");
        }

        // Use the current user's ID if no seller ID was provided
        var sellerId = request.SellerId;
        if (sellerId == Guid.Empty && _currentUserService.NameIdentifier != null)
        {
            if (Guid.TryParse(_currentUserService.NameIdentifier, out Guid userId))
            {
                sellerId = userId;
            }
            else
            {
                throw new InvalidOperationException("User NameIdentifier could not be determined from authenticated user");
            }
        }

        // Map command to domain entity
        var listingEntity = new ListingService.Domain.Entities.Listing
        {
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Location = request.Location,
            SellerId = sellerId,
            CategoryId = request.CategoryId,
            Status = ListingStatus.Draft,
            Views = 0,
            Tags = new List<Tag>(),
            Images = new List<ListingImage>()
        };
        
        // Get or create tags
        if (request.TagNames.Any())
        {
            foreach (var tagName in request.TagNames)
            {
                var tag = await _tagRepository.GetByNameAsync(tagName, cancellationToken);
                if (tag == null)
                {
                    tag = await _tagRepository.AddAsync(new Tag { Name = tagName }, cancellationToken);
                }
                
                listingEntity.Tags!.Add(tag);
            }
        }
        
        // Add to database first to get the ID
        var createdListing = await _listingRepository.AddAsync(listingEntity, cancellationToken);
        
        // Add images after we have the listing ID
        if (request.Images.Any())
        {
            foreach (var imageDto in request.Images)
            {
                var image = new ListingImage
                {
                    ListingId = createdListing.Id,
                    Url = imageDto.Url,
                    IsMainImage = imageDto.IsMainImage,
                    SortOrder = imageDto.SortOrder
                };
                
                createdListing.Images!.Add(image);
            }
            
            // Update with the images
            await _listingRepository.UpdateAsync(createdListing, cancellationToken);
        }
        
        // Get category for the event
        var category = await _categoryRepository.GetByIdAsync(createdListing.CategoryId, cancellationToken);
        
        // Publish event
        await _publishEndpoint.Publish(new ListingCreated(
            createdListing.Id,
            createdListing.Title,
            createdListing.Description ?? string.Empty,
            createdListing.Price,
            createdListing.Location ?? string.Empty,
            createdListing.SellerId,
            createdListing.CategoryId,
            category?.Name ?? string.Empty,
            createdListing.Tags?.Select(t => t.Name).ToArray() ?? Array.Empty<string>(),
            $"/api/listings/{createdListing.Id}",
            createdListing.IsActive,
            createdListing.CreatedAt
        ), cancellationToken);
        
        // Return mapped DTO
        return _mapper.Map<ListingDto>(createdListing);
    }
}