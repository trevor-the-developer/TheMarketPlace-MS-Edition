using MediatR;
using ListingService.Application.Features.Listing.Shared;

namespace ListingService.Application.Features.Listing.Commands.CreateListing;

public class CreateListingCommand : IRequest<ListingDto>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Location { get; set; }
    public Guid SellerId { get; set; }
    public Guid CategoryId { get; set; }
    public List<string> TagNames { get; set; } = new();
    public List<CreateListingImageCommand> Images { get; set; } = new();
}

public class CreateListingImageCommand
{
    public string Url { get; set; } = string.Empty;
    public bool IsMainImage { get; set; }
    public int SortOrder { get; set; }
}