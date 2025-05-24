using Services.Core.Entities;

namespace ListingService.Domain.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category>? SubCategories { get; set; }
    public ICollection<Listing>? Listings { get; set; }
}