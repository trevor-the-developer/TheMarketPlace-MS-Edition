using MediatR;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Queries.GetCategory;

public class GetCategoryQuery : IRequest<CategoryDto?>
{
    public Guid Id { get; set; }
}