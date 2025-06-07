using ListingService.Domain.Entities;

namespace ListingService.Application.Features.Listing.Shared;

public class ListingDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Location { get; set; }
    public Guid SellerId { get; set; }
    public ListingStatus Status { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<TagDto> Tags { get; set; } = new();
    public List<ListingImageDto> Images { get; set; } = new();
    public DateTime? PublishedAt { get; set; }
    public int Views { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ListingImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMainImage { get; set; }
    public int SortOrder { get; set; }
}