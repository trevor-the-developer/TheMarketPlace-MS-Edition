using SearchService.Application.Features.SearchResult;
using SearchService.Application.Models;
using Services.Core.Models;

namespace SearchService.Application.Contracts.Persistence;

public interface ISearchRepository
{
    Task<BasePagedResult<SearchResultDto>> SearchAsync(PagedRequestQuery pagedRequestQuery);
    Task SaveAsync(Item item);
}