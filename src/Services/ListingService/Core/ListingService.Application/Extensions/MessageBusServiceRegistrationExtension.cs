using System.Text.Json;
using System.Text.Json.Serialization;
using ListingService.Application.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ListingService.Application.Extensions;

public static class MessageBusServiceRegistrationExtension
{
    public static IServiceCollection AddMessageBusServices(this IServiceCollection services,
        ListingServiceConfiguration serviceConfiguration)
    {
        if (serviceConfiguration?.RabbitMQSettings == null)
        {
            throw new InvalidOperationException("RabbitMQSettings configuration is missing or null");
        }

        services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(serviceConfiguration.RabbitMQSettings.Host, "/", h =>
                {
                    h.Username(serviceConfiguration.RabbitMQSettings.Username);
                    h.Password(serviceConfiguration.RabbitMQSettings.Password);
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