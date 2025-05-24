namespace Services.Core.Extensions.Settings.RabbitMQ;

public record RabbitMQSettings
{
    public required string Host { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public int Port { get; init; } = 5672;
}