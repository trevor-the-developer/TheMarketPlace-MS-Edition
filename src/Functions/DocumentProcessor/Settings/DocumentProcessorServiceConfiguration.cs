using Services.Core.Extensions;
using Services.Core.Extensions.Settings.RabbitMQ;
using Services.Core.Extensions.Settings.MongoDb;
using Services.Core.Extensions.Settings.MinIO;
using Services.Core.Extensions.Settings.PostgresSql;

namespace DocumentProcessor.Settings;

public record DocumentProcessorServiceConfiguration : IApplicationConfiguration, IRabbitMQSettings, IMongoDbSettings, IMinIOSettings, IPostgresSqlSettings
{
    public required RabbitMQSettings RabbitMQSettings { get; init; }
    public required MongoDbSettings MongoDbSettings { get; init; }
    public required MinIOSettings MinIOSettings { get; init; }
    public required PostgresSqlSettings PostgresSqlSettings { get; init; }
}