using MediatR;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Queries.GetSubcategories;

public class GetSubcategoriesQuery : IRequest<List<CategoryDto>>
{
    public Guid ParentId { get; set; }
}