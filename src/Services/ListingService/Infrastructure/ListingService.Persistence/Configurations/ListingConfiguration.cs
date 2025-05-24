using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Entities;

namespace ListingService.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("Listings");
        
        builder.Property(l => l.Title)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(l => l.Description)
            .HasMaxLength(2000);
            
        builder.Property(l => l.Price)
            .HasPrecision(18, 2);
            
        builder.Property(l => l.Location)
            .HasMaxLength(100);
        
        // Add indices for common query patterns
        builder.HasIndex(l => l.Title);
        builder.HasIndex(l => l.CategoryId);
        builder.HasIndex(l => l.SellerId);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.PublishedAt);
    }
}