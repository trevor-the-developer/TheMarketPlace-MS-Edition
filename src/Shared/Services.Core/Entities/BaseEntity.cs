namespace Services.Core.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; init; }
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}