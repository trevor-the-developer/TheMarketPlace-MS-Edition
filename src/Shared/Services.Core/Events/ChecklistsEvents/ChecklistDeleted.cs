namespace Services.Core.Events.ChecklistsEvents;

public record ChecklistDeleted
{
    public Guid ChecklistId { get; init; }
    
    public Guid AccountId { get; init; }
}