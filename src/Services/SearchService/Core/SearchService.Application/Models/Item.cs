using MongoDB.Entities;
using System.Collections.Generic;
using Services.Core.Enums;

namespace SearchService.Application.Models;

public class Item : Entity
{
    public string Identifier { get; set; } = string.Empty;
    
    // e.g. Daniel Youd, Volvo M63 LRN
    public required string Name { get; set; }
    
    // e.g. Driver, Vehicle, Report, Checklist
    public required string Type { get; set; }
    
    // e.g. Guid
    public required string AccountId { get; set; }
    
    // e.g. /api/drivers/1234567890
    public required string ResourceUrl { get; set; } 
    
    public bool IsActive { get; set; }
    
    // DateTime (Utc)
    public DateTime CreatedAt { get; set; }
    
    // DateTime (Utc)
    public DateTime UpdatedAt { get; set; }
    
    // Additional properties to match consumer requirements
    public string Id { get => Identifier; set => Identifier = value; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    public ServiceStatus Status { get; set; }
}