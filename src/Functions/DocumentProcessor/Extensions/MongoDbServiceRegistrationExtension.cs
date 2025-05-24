using DocumentProcessor.Settings;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DocumentProcessor.Extensions;

public static class MongoDbServiceRegistrationExtension
{
    public static IServiceCollection AddMongoDbServices(this IServiceCollection services,
        DocumentProcessorServiceConfiguration serviceConfiguration)
    {
        services.AddSingleton<IMongoClient>(serviceProvider =>
            new MongoClient(serviceConfiguration.MongoDbSettings.ConnectionString));

        services.AddScoped<IMongoDatabase>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(serviceConfiguration.MongoDbSettings.DatabaseName);
        });

        return services;
    }
}