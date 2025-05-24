namespace SearchService.Api.RequestHelpers;

public class SearchParams
{
    public string SearchTerm { get; set; } = string.Empty;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 2;
    
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string OrderBy { get; set; } = string.Empty;
}