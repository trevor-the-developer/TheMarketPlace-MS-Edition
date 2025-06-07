using MediatR;
using Microsoft.AspNetCore.Mvc;
using Services.Core.Models.Service;
using Services.Core.Models;
using ListingService.Application.Features.Listing.Commands.CreateListing;
using ListingService.Application.Features.Listing.Commands.DeleteListing;
using ListingService.Application.Features.Listing.Commands.PublishListing;
using ListingService.Application.Features.Listing.Commands.UpdateListing;
using ListingService.Application.Features.Listing.Queries.GetListing;
using ListingService.Application.Features.Listing.Queries.GetListings;
using ListingService.Application.Features.Listing.Shared;
using Microsoft.AspNetCore.Authorization;
using ListingService.Application.Services.CurrentUserService;

namespace ListingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public ListingsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponseCollection<IReadOnlyCollection<ListingDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListings([FromQuery] GetListingsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.Data != null)
        {
            return Ok(ServiceResponseCollection<ListingDto>.Success(result.Data, result.TotalRecords, result.PageNumber,
                result.PageSize));
        }
        else
        {
            return NotFound();
        }
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponse<ListingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetListing(Guid id)
    {
        var result = await _mediator.Send(new GetListingQuery { Id = id });
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(ServiceResponse<ListingDto>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireSellerRole")]
    [ProducesResponseType(typeof(ServiceResponse<ListingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateListing([FromBody] CreateListingCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetListing), new { id = result.Id }, ServiceResponse<ListingDto>.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireSellerRole")]
    [ProducesResponseType(typeof(ServiceResponse<ListingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateListing(Guid id, [FromBody] UpdateListingCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(ServiceResponse<ListingDto>.Success(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireSellerRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteListing(Guid id)
    {
        var success = await _mediator.Send(new DeleteListingCommand { Id = id });
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "RequireSellerRole")]
    [ProducesResponseType(typeof(ServiceResponse<ListingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishListing(Guid id)
    {
        var result = await _mediator.Send(new PublishListingCommand { Id = id });
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(ServiceResponse<ListingDto>.Success(result));
    }

    [HttpGet("debug/claims")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetClaims()
    {
        var claims = _currentUserService.GetClaims().Select(c => new { Type = c.Type, Value = c.Value }).ToList();
        var result = new
        {
            IsAuthenticated = _currentUserService.IsAuthenticated,
            NameIdentifier = _currentUserService.NameIdentifier,
            Username = _currentUserService.Username,
            Email = _currentUserService.Email,
            Roles = _currentUserService.GetRoles(),
            Claims = claims
        };
        return Ok(result);
    }
}