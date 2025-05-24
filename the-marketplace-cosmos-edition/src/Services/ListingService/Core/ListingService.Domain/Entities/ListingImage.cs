using Services.Core.Entities;

namespace ListingService.Domain.Entities;

public class ListingImage : BaseEntity
{
    public required Guid ListingId { get; set; }
    public required string Url { get; set; }
    public bool IsMainImage { get; set; }
    public int SortOrder { get; set; }
    public Listing? Listing { get; set; }
}