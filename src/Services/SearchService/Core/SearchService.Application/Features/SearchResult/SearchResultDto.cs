namespace SearchService.Application.Features.SearchResult;

public class SearchResultDto
{
    public string Identifier { get; set; }
    
    // e.g. Antique Desk
    public required string Name { get; set; }
    
    // e.g. Listing, Checklist
    public required string Type { get; set; }
    
    // e.g. Guid
    public required string AccountId { get; set; }
    
    // e.g. /api/listings/1234567890
    public required string ResourceUrl { get; set; } 
    
    public bool IsActive { get; set; }
    
    // DateTime (Utc)
    public DateTime CreatedAt { get; set; }
    
    // DateTime (Utc)
    public DateTime UpdatedAt { get; set; }
}