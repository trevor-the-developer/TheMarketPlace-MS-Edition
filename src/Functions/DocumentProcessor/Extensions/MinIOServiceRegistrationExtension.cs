using DocumentProcessor.Settings;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace DocumentProcessor.Extensions;

public static class MinIOServiceRegistrationExtension
{
    public static IServiceCollection AddMinIOServices(this IServiceCollection services,
        DocumentProcessorServiceConfiguration serviceConfiguration)
    {
        services.AddSingleton<IMinioClient>(serviceProvider =>
        {
            return new MinioClient()
                .WithEndpoint(serviceConfiguration.MinIOSettings.Endpoint)
                .WithCredentials(serviceConfiguration.MinIOSettings.AccessKey, serviceConfiguration.MinIOSettings.SecretKey)
                .WithSSL(serviceConfiguration.MinIOSettings.UseSSL)
                .Build();
        });

        return services;
    }
}