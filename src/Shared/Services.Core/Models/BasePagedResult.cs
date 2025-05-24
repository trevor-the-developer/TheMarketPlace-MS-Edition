namespace Services.Core.Models;

public class BasePagedResult<T>
{
    public IReadOnlyCollection<T>? Data { get; init; }
    
    public int TotalRecords { get; init; }
    
    public int TotalCount => TotalRecords;
    
    public int TotalPages { get; init; }
    
    public int PageNumber { get; init; }
    
    public int PageSize { get; init; }
}