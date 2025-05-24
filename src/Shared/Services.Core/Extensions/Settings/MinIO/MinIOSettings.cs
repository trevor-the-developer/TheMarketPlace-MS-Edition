namespace Services.Core.Extensions.Settings.MinIO;

public record MinIOSettings
{
    public required string Endpoint { get; init; }
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public bool UseSSL { get; init; } = false;
}