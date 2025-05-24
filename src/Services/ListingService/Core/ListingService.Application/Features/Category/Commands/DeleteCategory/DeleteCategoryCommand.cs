using MediatR;

namespace ListingService.Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}