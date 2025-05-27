using ListingService.Application.Extensions;
using ListingService.Application.Settings;
using ListingService.Persistence.DatabaseContext;
using ListingService.Persistence.Extensions;
using ListingService.Persistence.SeedData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services.Core.Extensions;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var appConfig = builder.Services.AddApplicationConfiguration<ListingServiceConfiguration>(builder.Configuration);
var configuration = builder.Configuration;

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))
            ),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireSellerRole", policy => policy.RequireRole("Seller"));
});

builder.Services
    .AddSingleton(appConfig)
    .AddApplicationServices()
    .AddPersistenceServices(appConfig)
    .AddMessageBusServices(appConfig)
    .AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

var app = builder.Build();

// Apply migrations and seed data - moved outside of Development check
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ListingDatabaseContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database initialization...");
        
        // Check if database exists and can connect
        logger.LogInformation("Checking database connection...");
        var canConnect = await dbContext.Database.CanConnectAsync();
        logger.LogInformation($"Database connection status: {canConnect}");
        
        // Get pending migrations
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        logger.LogInformation($"Pending migrations count: {pendingMigrations.Count()}");
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Pending migrations found:");
            foreach (var migration in pendingMigrations)
            {
                logger.LogInformation($"  - {migration}");
            }
        }
        
        // Apply migrations
        logger.LogInformation("Applying database migrations...");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");
        
        // Verify tables exist
        logger.LogInformation("Verifying database schema...");
        var tableExists = await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
        logger.LogInformation("Database schema verification completed");
        
        // Check if Categories table exists and has data
        try
        {
            var categoryCount = await dbContext.Categories.CountAsync();
            logger.LogInformation($"Categories table exists with {categoryCount} records");
            
            // Seed data only if no categories exist
            if (categoryCount == 0)
            {
                logger.LogInformation("No categories found, seeding category data...");
                await SeedCategoryData.SeedAsync(dbContext, logger);
                logger.LogInformation("Category data seeding completed");
            }
            else
            {
                logger.LogInformation("Categories already exist, skipping seeding");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Categories table does not exist or seeding failed");
            throw; // Re-throw to prevent service from starting with bad state
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization");
        throw; // Re-throw to prevent service from starting with incomplete database
    }
}

// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add authentication middleware before authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseCors();
app.MapControllers();

app.Run();

namespace ListingService.Api
{
    public partial class Program { }
}