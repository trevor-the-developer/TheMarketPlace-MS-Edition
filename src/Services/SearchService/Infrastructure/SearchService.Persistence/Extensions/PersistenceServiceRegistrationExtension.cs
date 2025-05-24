using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using SearchService.Application.Contracts.Persistence;
using SearchService.Application.Settings;
using SearchService.Persistence.Repositories;

namespace SearchService.Persistence.Extensions;

public static class PersistenceServiceRegistrationExtension
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, 
        SearchServiceConfiguration serviceConfiguration)
    {
        // Configure OpenSearch
        services.AddSingleton<IOpenSearchClient>(sp =>
        {
            var uri = new Uri(serviceConfiguration.OpenSearchSettings.Uri);
            var settings = new ConnectionSettings(uri)
                .DefaultIndex("marketplace")
                .EnableDebugMode()
                .PrettyJson();
            
            return new OpenSearchClient(settings);
        });
        
        services.AddScoped<ISearchRepository, OpenSearchRepository>();
        
        return services;
    }
}
