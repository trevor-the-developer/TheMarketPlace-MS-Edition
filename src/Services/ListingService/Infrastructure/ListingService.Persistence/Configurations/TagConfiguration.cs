using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ListingService.Domain.Entities;

namespace ListingService.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags");
        
        builder.Property(t => t.Name)
            .HasMaxLength(50)
            .IsRequired();
            
        // Add a unique index on Name to prevent duplicates
        builder.HasIndex(t => t.Name).IsUnique();
    }
}