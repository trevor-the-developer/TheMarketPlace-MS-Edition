using System.Text.Json;
using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using SearchService.Application.Consumers;
using SearchService.Application.Settings;
using Services.Core.ServiceBus;

namespace SearchService.Application.Extensions;

public static class MessageBusServiceRegistrationExtension
{
    public static IServiceCollection AddMessageBusServices(this IServiceCollection services,
        SearchServiceConfiguration serviceConfiguration)
    {
        services.AddMassTransit(config =>
        {
            // Register consumers
            config.AddConsumer<DriverCreatedConsumer>();
            config.AddConsumer<ListingCreatedConsumer>();

            config.UsingAzureServiceBus((context, cfg) =>
            {
                cfg.Host(serviceConfiguration.AzureServiceBusSettings.ConnectionString);
                
                // Driver events
                cfg.SubscriptionEndpoint(
                    subscriptionName: ServiceBusConstants.Topics.Driver.Subscriptions.SearchService,
                    topicPath: ServiceBusConstants.Topics.Driver.Created, 
                    configure: e =>
                    {
                        e.ConfigureConsumer<DriverCreatedConsumer>(context);
                    });
                
                // Listing events
                cfg.SubscriptionEndpoint(
                    subscriptionName: ServiceBusConstants.Topics.Listing.Subscriptions.SearchServiceCreated,
                    topicPath: ServiceBusConstants.Topics.Listing.Created, 
                    configure: e =>
                    {
                        e.ConfigureConsumer<ListingCreatedConsumer>(context);
                    });
                
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