namespace Services.Core.Extensions.Settings.AzureServiceBus;

public record AzureServiceBusSettings
{
    public required string ConnectionString { get; init; }
}