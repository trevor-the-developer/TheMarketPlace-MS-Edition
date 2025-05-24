using MediatR;
using SearchService.Application.Contracts.Persistence;
using Services.Core.Enums;
using Services.Core.Models;
using Services.Core.Models.Service;

namespace SearchService.Application.Features.SearchResult.Queries.GetSearchResults;

public class GetSearchResultsQueryHandler(ISearchRepository searchRepository)
    : IRequestHandler<GetSearchResultsQuery, ServiceResponseCollection<BasePagedResult<SearchResultDto>>>
{
    public async Task<ServiceResponseCollection<BasePagedResult<SearchResultDto>>> Handle(GetSearchResultsQuery request, CancellationToken cancellationToken)
    {
        var results = await searchRepository.SearchAsync(request.PagedRequestQuery);

        return new ServiceResponseCollection<BasePagedResult<SearchResultDto>>()
        {
            Data = results,
            TotalRecords = results.TotalRecords,
            Status = ServiceStatus.Success,
            Message = "Success"
        };
    }
}