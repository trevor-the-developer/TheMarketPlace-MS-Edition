using MediatR;

namespace ListingService.Application.Features.Listing.Commands.DeleteListing;

public class DeleteListingCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}