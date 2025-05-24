namespace Services.Core.Extensions.Settings.MongoDb;

public record MongoDbSettings
{
    public required string ConnectionString { get; init; }
    
    public required string DatabaseName { get; init; }
}