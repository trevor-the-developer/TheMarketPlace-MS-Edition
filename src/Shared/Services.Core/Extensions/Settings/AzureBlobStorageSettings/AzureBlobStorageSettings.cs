namespace Services.Core.Extensions.Settings.AzureBlobStorageSettings;

public record AzureBlobStorageSettings
{
    public required string ConnectionString { get; init; }
}