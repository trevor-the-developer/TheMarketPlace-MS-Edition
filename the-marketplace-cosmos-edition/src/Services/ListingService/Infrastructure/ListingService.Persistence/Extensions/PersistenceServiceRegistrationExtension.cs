using ListingService.Application.Contracts.Persistence;
using ListingService.Application.Settings;
using ListingService.Persistence.DatabaseContext;
using ListingService.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ListingService.Persistence.Extensions;

public static class PersistenceServiceRegistrationExtension
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services,
        ListingServiceConfiguration serviceConfiguration)
    {
        // Register DbContext
        services.AddDbContext<ListingDatabaseContext>(options =>
            options.UseNpgsql(serviceConfiguration.PostgresSqlSettings.ConnectionString));
        
        // Register repositories
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        
        return services;
    }
}