using System.Configuration;
using Azure.Storage.Blobs;
using DocumentProcessor.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DocumentProcessor.Extensions;

public static class BlobStorageServiceRegistrationExtension
{
    public static IServiceCollection AddBlobStorageServices(this IServiceCollection services,
        DocumentProcessorServiceConfiguration serviceConfiguration)
    {
        services.AddSingleton<BlobServiceClient>(serviceProvider => 
            new BlobServiceClient(serviceConfiguration.AzureBlobStorageSettings.ConnectionString));
        
        return services;
    }
}