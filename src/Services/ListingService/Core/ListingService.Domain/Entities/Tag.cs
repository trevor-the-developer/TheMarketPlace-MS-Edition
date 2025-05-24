using Services.Core.Entities;

namespace ListingService.Domain.Entities;

public class Tag : BaseEntity
{
    public required string Name { get; set; }
    public ICollection<Listing>? Listings { get; set; }
}