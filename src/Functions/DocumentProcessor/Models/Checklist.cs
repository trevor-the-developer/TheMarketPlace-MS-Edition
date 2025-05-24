namespace DocumentProcessor.Models;

public class Checklist
{
    public Guid id { get; set; }
    
    public Guid AccountId { get; set; }
    
    public string? Title { get; set; }
    
    public bool IsSubmitted { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}