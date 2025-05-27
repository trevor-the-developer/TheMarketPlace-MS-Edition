using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthenticationService.Persistence.SeedData;

public static class SeedRoleData
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        try
        {
            logger.LogInformation("Checking for existing roles...");
            
            // Define roles with specific IDs to match our testing environment
            var roles = new List<(string Id, string Name)>
            {
                ("1", "Admin"),
                ("2", "User"),
                ("3", "Seller")
            };

            foreach (var (id, name) in roles)
            {
                // Check if role exists
                var roleExists = await roleManager.RoleExistsAsync(name);
                
                if (!roleExists)
                {
                    logger.LogInformation("Creating role: {RoleName} with ID: {RoleId}", name, id);
                    
                    // Create role with specific ID
                    var role = new IdentityRole
                    {
                        Id = id,
                        Name = name,
                        NormalizedName = name.ToUpperInvariant(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };
                    
                    var result = await roleManager.CreateAsync(role);
                    
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Role {RoleName} created successfully", name);
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError("Failed to create role {RoleName}. Errors: {Errors}", name, errors);
                    }
                }
                else
                {
                    logger.LogInformation("Role {RoleName} already exists", name);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding roles");
        }
    }
}

