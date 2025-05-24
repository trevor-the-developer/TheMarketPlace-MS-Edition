using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentProcessor.Consumers;
using DocumentProcessor.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Services.Core.ServiceBus;

namespace DocumentProcessor.Extensions;

public static class MessageBusServiceRegistrationExtension
{
    public static IServiceCollection AddMessageBusServices(this IServiceCollection services,
        DocumentProcessorServiceConfiguration serviceConfiguration)
    {
        services.AddMassTransit(config =>
        {
            config.AddConsumer<ChecklistSubmittedConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(serviceConfiguration.RabbitMQSettings.Host, "/", h =>
                {
                    h.Username(serviceConfiguration.RabbitMQSettings.Username);
                    h.Password(serviceConfiguration.RabbitMQSettings.Password);
                });

                cfg.ReceiveEndpoint(ServiceBusConstants.Topics.Checklist.Subscriptions.DocumentProcessorSubmitted, e =>
                {
                    e.ConfigureConsumer<ChecklistSubmittedConsumer>(context);
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