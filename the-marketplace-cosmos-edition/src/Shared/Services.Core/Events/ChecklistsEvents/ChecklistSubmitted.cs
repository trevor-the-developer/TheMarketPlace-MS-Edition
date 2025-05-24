using System.Text.Json.Serialization;

namespace Services.Core.Events.ChecklistsEvents;

public record ChecklistSubmitted
{
    [JsonPropertyName("checklistId")]
    public Guid ChecklistId { get; init; }
    
    [JsonPropertyName("accountId")]
    public Guid AccountId { get; init; }
    
    [JsonPropertyName("submittedAt")]
    public DateTime SubmittedAt { get; init; }
}
