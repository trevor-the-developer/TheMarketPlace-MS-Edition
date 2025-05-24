using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Entities;

namespace ListingService.Persistence.Configurations;

public class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        builder.ToTable("ListingImages");
        
        builder.Property(i => i.Url)
            .HasMaxLength(500)
            .IsRequired();
            
        // Add an index for the ListingId for faster image retrieval
        builder.HasIndex(i => i.ListingId);
    }
}