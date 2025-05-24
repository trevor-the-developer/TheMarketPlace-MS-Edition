using System.Text.Json.Serialization;

namespace Services.Core.Events.ChecklistsEvents;

public record ChecklistCreated
{
    [JsonPropertyName("checklistId")]
    public Guid ChecklistId { get; init; }
    
    [JsonPropertyName("accountId")]
    public Guid AccountId { get; init; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }
}