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

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(serviceConfiguration.RabbitMQSettings.Host, "/", h =>
                {
                    h.Username(serviceConfiguration.RabbitMQSettings.Username);
                    h.Password(serviceConfiguration.RabbitMQSettings.Password);
                });
                
                // Driver events
                cfg.ReceiveEndpoint(ServiceBusConstants.Topics.Driver.Subscriptions.SearchService, e =>
                {
                    e.ConfigureConsumer<DriverCreatedConsumer>(context);
                });
                
                // Listing events
                cfg.ReceiveEndpoint(ServiceBusConstants.Topics.Listing.Subscriptions.SearchServiceCreated, e =>
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