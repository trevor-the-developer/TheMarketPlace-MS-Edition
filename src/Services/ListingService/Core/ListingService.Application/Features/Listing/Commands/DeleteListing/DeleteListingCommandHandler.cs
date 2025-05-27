using MediatR;
using MassTransit;
using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Services.CurrentUserService;
using Services.Core.Events.ListingEvents;

namespace ListingService.Application.Features.Listing.Commands.DeleteListing;

public class DeleteListingCommandHandler : IRequestHandler<DeleteListingCommand, bool>
{
    private readonly IListingRepository _listingRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICurrentUserService _currentUserService;

    public DeleteListingCommandHandler(
        IListingRepository listingRepository,
        IPublishEndpoint publishEndpoint,
        ICurrentUserService currentUserService)
    {
        _listingRepository = listingRepository;
        _publishEndpoint = publishEndpoint;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User must be authenticated to delete listings");
        }

        if (!_currentUserService.IsInRole("Seller"))
        {
            throw new UnauthorizedAccessException("User must have the Seller role to delete listings");
        }

        var listing = await _listingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (listing == null)
        {
            return false;
        }

        // Verify the user owns this listing
        var currentUserId = _currentUserService.NameIdentifier;
        if (currentUserId != null && Guid.TryParse(currentUserId, out Guid userId))
        {
            if (listing.SellerId != userId)
            {
                throw new UnauthorizedAccessException("User can only delete their own listings");
            }
        }
        else
        {
            throw new InvalidOperationException("User NameIdentifier could not be determined from authenticated user");
        }

        await _listingRepository.DeleteAsync(listing.Id, cancellationToken);

        // Publish event
        await _publishEndpoint.Publish(new ListingDeleted(
            listing.Id
        ), cancellationToken);

        return true;
    }
}