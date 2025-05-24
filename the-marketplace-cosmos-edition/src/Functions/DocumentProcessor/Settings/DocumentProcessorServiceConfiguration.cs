using Services.Core.Extensions;
using Services.Core.Extensions.Settings.AzureBlobStorageSettings;
using Services.Core.Extensions.Settings.AzureCosmosDbSettings;
using Services.Core.Extensions.Settings.AzureServiceBus;

namespace DocumentProcessor.Settings;

public record DocumentProcessorServiceConfiguration : IApplicationConfiguration, IAzureServiceBusSettings, IAzureCosmosDbSettings, IAzureBlobStorageSettings
{
    public required AzureServiceBusSettings AzureServiceBusSettings { get; init; }
    public required AzureCosmosDbSettings AzureCosmosDbSettings { get; init; }
    public required AzureBlobStorageSettings AzureBlobStorageSettings { get; init; }
}