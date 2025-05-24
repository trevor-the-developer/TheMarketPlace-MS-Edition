using MediatR;
using ListingService.Application.Features.Listing.Shared;

namespace ListingService.Application.Features.Listing.Commands.UpdateListing;

public class UpdateListingCommand : IRequest<ListingDto>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Location { get; set; }
    public Guid CategoryId { get; set; }
    public List<string> TagNames { get; set; } = new();
    public List<UpdateListingImageCommand> Images { get; set; } = new();
}

public class UpdateListingImageCommand
{
    public Guid? Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMainImage { get; set; }
    public int SortOrder { get; set; }
}