using MediatR;
using Services.Core.Models;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Queries.GetCategories;

public class GetCategoriesQuery : PagedRequestQuery, IRequest<BasePagedResult<CategoryDto>>
{
}