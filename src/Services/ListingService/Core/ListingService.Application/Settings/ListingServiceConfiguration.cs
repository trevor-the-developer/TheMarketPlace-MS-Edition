using Services.Core.Extensions;
using Services.Core.Extensions.Settings.RabbitMQ;
using Services.Core.Extensions.Settings.PostgresSql;

namespace ListingService.Application.Settings;

public record ListingServiceConfiguration : IApplicationConfiguration, IRabbitMQSettings, IPostgresSqlSettings
{
    public required RabbitMQSettings RabbitMQSettings { get; init; }
    public required PostgresSqlSettings PostgresSqlSettings { get; init; }
}