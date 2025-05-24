using System.Text.Json;
using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using ListingService.Application.Settings;

namespace ListingService.Application.Extensions;

public static class MessageBusServiceRegistrationExtension
{
    public static IServiceCollection AddMessageBusServices(this IServiceCollection services,
        ListingServiceConfiguration serviceConfiguration)
    {
        services.AddMassTransit(config =>
        {
            config.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(serviceConfiguration.AzureServiceBusSettings.ConnectionString);

                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    return options;
                });
            });
        });
        
        return services;
    }
}