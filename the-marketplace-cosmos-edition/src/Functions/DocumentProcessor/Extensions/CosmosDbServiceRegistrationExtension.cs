using DocumentProcessor.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentProcessor.Extensions;

public static class CosmosDbServiceRegistrationExtension
{
    public static IServiceCollection AddCosmosDbServices(this IServiceCollection services,
        DocumentProcessorServiceConfiguration serviceConfiguration)
    {
        services.AddSingleton<CosmosClient>((serviceProvider) =>
        {
            CosmosClient client = new(
                serviceConfiguration.AzureCosmosDbSettings.AccountEndpoint, 
                serviceConfiguration.AzureCosmosDbSettings.AccountKey
            );
    
            return client;
        });
        
        return services;
    }
}