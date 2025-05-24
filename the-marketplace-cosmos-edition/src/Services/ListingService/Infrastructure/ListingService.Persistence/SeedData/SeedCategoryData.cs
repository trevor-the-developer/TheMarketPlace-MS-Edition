using ListingService.Domain.Entities;
using ListingService.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ListingService.Persistence.SeedData;

public class SeedCategoryData
{
    public static async Task SeedAsync(ListingDatabaseContext context, ILogger logger)
    {
        try
        {
            // Only seed if no categories exist
            if (!await context.Categories.AnyAsync())
            {
                logger.LogInformation("Seeding categories data");
                
                // Create root categories
                var electronics = new Category { Name = "Electronics", Description = "Electronic devices and gadgets" };
                var clothing = new Category { Name = "Clothing", Description = "Apparel and fashion items" };
                var homeGarden = new Category { Name = "Home & Garden", Description = "Home decor, furniture, and garden supplies" };
                var vehicles = new Category { Name = "Vehicles", Description = "Cars, motorcycles, and other vehicles" };
                
                await context.Categories.AddRangeAsync(new[] { electronics, clothing, homeGarden, vehicles });
                await context.SaveChangesAsync();
                
                // Create subcategories for Electronics
                var smartphones = new Category { Name = "Smartphones", Description = "Mobile phones and accessories", ParentCategoryId = electronics.Id };
                var computers = new Category { Name = "Computers", Description = "Laptops, desktops, and components", ParentCategoryId = electronics.Id };
                var audioVideo = new Category { Name = "Audio & Video", Description = "TVs, speakers, and audio equipment", ParentCategoryId = electronics.Id };
                
                // Create subcategories for Clothing
                var menClothing = new Category { Name = "Men's Clothing", Description = "Clothing for men", ParentCategoryId = clothing.Id };
                var womenClothing = new Category { Name = "Women's Clothing", Description = "Clothing for women", ParentCategoryId = clothing.Id };
                var accessories = new Category { Name = "Accessories", Description = "Hats, belts, and other accessories", ParentCategoryId = clothing.Id };
                
                // Create subcategories for Home & Garden
                var furniture = new Category { Name = "Furniture", Description = "Tables, chairs, sofas, and more", ParentCategoryId = homeGarden.Id };
                var kitchenAppliances = new Category { Name = "Kitchen Appliances", Description = "Cookware, cutlery, and small appliances", ParentCategoryId = homeGarden.Id };
                var gardenSupplies = new Category { Name = "Garden Supplies", Description = "Tools, plants, and outdoor furniture", ParentCategoryId = homeGarden.Id };
                
                // Create subcategories for Vehicles
                var cars = new Category { Name = "Cars", Description = "Automobiles for sale", ParentCategoryId = vehicles.Id };
                var motorcycles = new Category { Name = "Motorcycles", Description = "Motorcycles and scooters", ParentCategoryId = vehicles.Id };
                var parts = new Category { Name = "Parts & Accessories", Description = "Vehicle parts and accessories", ParentCategoryId = vehicles.Id };
                
                await context.Categories.AddRangeAsync(new[] 
                { 
                    smartphones, computers, audioVideo,
                    menClothing, womenClothing, accessories,
                    furniture, kitchenAppliances, gardenSupplies,
                    cars, motorcycles, parts
                });
                
                await context.SaveChangesAsync();
                
                logger.LogInformation("Category seed data completed");
            }
            else
            {
                logger.LogInformation("Category data already exists, skipping seed");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding category data");
        }
    }
}