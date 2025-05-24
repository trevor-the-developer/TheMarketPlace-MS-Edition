using Services.Core.Extensions;
using Services.Core.Extensions.Settings.RabbitMQ;
using Services.Core.Extensions.Settings.MongoDb;

namespace SearchService.Application.Settings;

public record SearchServiceConfiguration : IApplicationConfiguration, IRabbitMQSettings, IMongoDbSettings
{
    public required RabbitMQSettings RabbitMQSettings { get; init; }
    public required MongoDbSettings MongoDbSettings { get; init; }
    public required OpenSearchSettings OpenSearchSettings { get; init; }
}