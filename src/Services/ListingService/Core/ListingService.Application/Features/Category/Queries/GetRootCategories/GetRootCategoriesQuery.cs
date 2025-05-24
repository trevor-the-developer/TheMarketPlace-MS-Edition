using MediatR;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Queries.GetRootCategories;

public class GetRootCategoriesQuery : IRequest<List<CategoryDto>>
{
}