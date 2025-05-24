using MediatR;
using ListingService.Application.Features.Listing.Shared;

namespace ListingService.Application.Features.Listing.Commands.PublishListing;

public class PublishListingCommand : IRequest<ListingDto?>
{
    public Guid Id { get; set; }
}