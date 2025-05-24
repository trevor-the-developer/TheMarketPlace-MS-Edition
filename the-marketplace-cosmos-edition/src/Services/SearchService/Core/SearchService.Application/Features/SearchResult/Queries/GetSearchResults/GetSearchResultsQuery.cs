using Services.Core.Models;
using MediatR;
using Services.Core.Models.Service;

namespace SearchService.Application.Features.SearchResult.Queries.GetSearchResults;

public record GetSearchResultsQuery(PagedRequestQuery PagedRequestQuery) 
    : IRequest<ServiceResponseCollection<BasePagedResult<SearchResultDto>>>;