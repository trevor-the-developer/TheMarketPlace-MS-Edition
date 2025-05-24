using MediatR;
using ListingService.Application.Features.Listing.Shared;

namespace ListingService.Application.Features.Listing.Queries.GetListing;

public class GetListingQuery : IRequest<ListingDto?>
{
    public Guid Id { get; set; }
}