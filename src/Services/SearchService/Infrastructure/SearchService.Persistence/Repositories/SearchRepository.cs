using SearchService.Application.Contracts.Persistence;
using SearchService.Application.Features.SearchResult;
using SearchService.Application.Models;
using SearchService.Persistence.DatabaseContext;
using Services.Core.Models;

namespace SearchService.Persistence.Repositories;

public class SearchRepository(ISearchServiceDatabaseContext context) : ISearchRepository
{
    public async Task<BasePagedResult<SearchResultDto>> SearchAsync(PagedRequestQuery pagedRequestQuery)
    {
        var query = await context.FindAsync<Item>(item => 
            item.Name.Contains(pagedRequestQuery.SearchBy ?? string.Empty) || 
            item.Type.Contains(pagedRequestQuery.SearchBy ?? string.Empty));
        
        // todo: Add paging, orderby etc
        // var query = DB.PagedSearch<Item, Item>();
        //
        // if (!string.IsNullOrWhiteSpace(searchParams.SearchTerm)) 
        // {
        //     query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        // }
        //
        // query = searchParams.OrderBy switch
        // {
        //     "name" => query.Sort(x => x.Ascending(i => i.Name)),
        //     "type" => query.Sort(x => x.Ascending(i => i.Type)),
        //     _ => query.Sort(x => x.Descending(i => i.UpdatedAt))
        // };
        //
        // if (!string.IsNullOrEmpty(searchParams.Name))
        // {
        //     query.Match(x => x.Name.Contains(searchParams.Name));
        // }
        //
        // if (!string.IsNullOrEmpty(searchParams.Type))
        // {
        //     query.Match(x => x.Type.Contains(searchParams.Type));
        // }
        //
        // query.PageNumber(searchParams.PageNumber);
        // query.PageSize(searchParams.PageSize);
        //
        // var results = await query.ExecuteAsync();

        var results = query.Select(item => new SearchResultDto
        {
            Identifier = item.Identifier,
            Name = item.Name,
            Type = item.Type,
            AccountId = item.AccountId,
            ResourceUrl = item.ResourceUrl,
            IsActive = item.IsActive,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        }).ToList();

        return new BasePagedResult<SearchResultDto>
        {
            Data = results,
            TotalRecords = results.Count, // todo: when adding paging need to get total count from db
            PageNumber = pagedRequestQuery.PageNumber,
            PageSize = pagedRequestQuery.PageSize,
            TotalPages = (int)Math.Ceiling(results.Count / (double)pagedRequestQuery.PageSize)
        };
    }
    
    public async Task SaveAsync(Item item)
    {
        await context.SaveAsync(item);
    }
}