using System.Text.Json.Serialization;

namespace DocumentProcessor.Models;

public class MassTransitEnvelope<T>
{
    [JsonPropertyName("messageId")]
    public required string MessageId { get; set; }
    
    [JsonPropertyName("messageType")]
    public required string[] MessageType { get; set; }
    
    [JsonPropertyName("message")]
    public required T Message { get; set; }
    
    [JsonPropertyName("sentTime")]
    public DateTime? SentTime { get; set; }
}