namespace Services.Core.Extensions.Settings.AzureCosmosDbSettings;

public record AzureCosmosDbSettings
{
    public required string AccountEndpoint { get; init; }
    
    public required string AccountKey { get; init; }

    public required string DatabaseName { get; init; }

    public required string ContainerName { get; init; }
}