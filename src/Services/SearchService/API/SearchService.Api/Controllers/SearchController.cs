using MediatR;
using Microsoft.AspNetCore.Mvc;
using SearchService.Application.Features.SearchResult.Queries.GetSearchResults;
using SearchService.Application.Models;
using Services.Core.Models;

namespace SearchService.Api.Controllers;

// todo: Add Authorize attribute
[ApiController]
[Route("api/search")]
public class SearchController(ISender mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<Item>>> SearchItems(
        [FromQuery] PagedRequestQuery pagedRequestQuery)
    {
        var searchResults = await mediator.Send(new GetSearchResultsQuery(pagedRequestQuery));
        
        return Ok(searchResults);
    }
}