using Services.Core.Entities;

namespace ListingService.Domain.Entities;

public class Listing : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Location { get; set; }
    public Guid SellerId { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Draft;
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<Tag>? Tags { get; set; }
    public ICollection<ListingImage>? Images { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int Views { get; set; }
}

public enum ListingStatus
{
    Draft,
    Published,
    Sold,
    Archived
}