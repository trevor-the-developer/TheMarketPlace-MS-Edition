using Services.Core.Extensions;
using Services.Core.Extensions.Settings.AzureServiceBus;
using Services.Core.Extensions.Settings.MongoDb;

namespace SearchService.Application.Settings;

public record SearchServiceConfiguration : IApplicationConfiguration, IAzureServiceBusSettings, IMongoDbSettings
{
    public required AzureServiceBusSettings AzureServiceBusSettings { get; init; }
    public required MongoDbSettings MongoDbSettings { get; init; }
    public required OpenSearchSettings OpenSearchSettings { get; init; }
}