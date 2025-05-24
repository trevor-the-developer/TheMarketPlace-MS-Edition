using Microsoft.EntityFrameworkCore;
using Services.Core.DatabaseContext;
using ListingService.Domain.Entities;

namespace ListingService.Persistence.DatabaseContext;

public class ListingDatabaseContext : BaseDatabaseContext<ListingDatabaseContext>
{
    public ListingDatabaseContext(DbContextOptions<ListingDatabaseContext> options) : base(options)
    {
    }

    public DbSet<Listing> Listings { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ListingImage> ListingImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ListingDatabaseContext).Assembly);
        
        // Many-to-many relationship between Listing and Tag
        modelBuilder.Entity<Listing>()
            .HasMany(l => l.Tags)
            .WithMany(t => t.Listings)
            .UsingEntity(j => j.ToTable("ListingTags"));
            
        // One-to-many relationship between Category and Listing
        modelBuilder.Entity<Listing>()
            .HasOne(l => l.Category)
            .WithMany(c => c.Listings)
            .HasForeignKey(l => l.CategoryId);
            
        // One-to-many relationship between Category and SubCategories
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .IsRequired(false);
            
        // One-to-many relationship between Listing and ListingImage
        modelBuilder.Entity<ListingImage>()
            .HasOne(i => i.Listing)
            .WithMany(l => l.Images)
            .HasForeignKey(i => i.ListingId);
    }
}