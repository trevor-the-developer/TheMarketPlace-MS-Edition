using MediatR;
using Microsoft.AspNetCore.Mvc;
using Services.Core.Models.Service;
using Services.Core.Models;
using ListingService.Application.Features.Category.Commands.CreateCategory;
using ListingService.Application.Features.Category.Commands.UpdateCategory;
using ListingService.Application.Features.Category.Commands.DeleteCategory;
using ListingService.Application.Features.Category.Queries.GetCategory;
using ListingService.Application.Features.Category.Queries.GetCategories;
using ListingService.Application.Features.Category.Queries.GetRootCategories;
using ListingService.Application.Features.Category.Queries.GetSubcategories;
using ListingService.Application.Features.Category.Shared;
using Microsoft.AspNetCore.Authorization;

namespace ListingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponseCollection<IReadOnlyCollection<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] GetCategoriesQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.Data != null)
        {
            return Ok(ServiceResponseCollection<CategoryDto>.Success(result.Data, result.TotalRecords, result.PageNumber, result.PageSize));            
        }
        else
        {
            return NotFound(result.Data);
        }
    }

    [HttpGet("root")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRootCategories()
    {
        var result = await _mediator.Send(new GetRootCategoriesQuery());
        return Ok(ServiceResponse<List<CategoryDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var result = await _mediator.Send(new GetCategoryQuery { Id = id });
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(ServiceResponse<CategoryDto>.Success(result));
    }

    [HttpGet("{id:guid}/subcategories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ServiceResponse<List<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubcategories(Guid id)
    {
        var result = await _mediator.Send(new GetSubcategoriesQuery { ParentId = id });
        return Ok(ServiceResponse<List<CategoryDto>>.Success(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ServiceResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCategory), new { id = result.Id }, ServiceResponse<CategoryDto>.Success(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(ServiceResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(ServiceResponse<CategoryDto>.Success(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var success = await _mediator.Send(new DeleteCategoryCommand { Id = id });
        
        if (!success)
        {
            return NotFound();
        }
        
        return NoContent();
    }
}