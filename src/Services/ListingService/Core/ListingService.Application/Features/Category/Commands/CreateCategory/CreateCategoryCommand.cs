using MediatR;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<CategoryDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}