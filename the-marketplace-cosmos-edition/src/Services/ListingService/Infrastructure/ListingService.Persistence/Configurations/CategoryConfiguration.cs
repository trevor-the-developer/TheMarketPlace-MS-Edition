using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Entities;

namespace ListingService.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        
        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();
            
        builder.Property(c => c.Description)
            .HasMaxLength(500);
            
        // Add index on Name for faster lookup
        builder.HasIndex(c => c.Name);
    }
}