using MediatR;
using Services.Core.Models;
using ListingService.Application.Features.Listing.Shared;

namespace ListingService.Application.Features.Listing.Queries.GetListings;

public class GetListingsQuery : PagedRequestQuery, IRequest<BasePagedResult<ListingDto>>
{
    public Guid? CategoryId { get; set; }
    public Guid? SellerId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Location { get; set; }
}