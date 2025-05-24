using MediatR;
using ListingService.Application.Features.Category.Shared;

namespace ListingService.Application.Features.Category.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest<CategoryDto?>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
}