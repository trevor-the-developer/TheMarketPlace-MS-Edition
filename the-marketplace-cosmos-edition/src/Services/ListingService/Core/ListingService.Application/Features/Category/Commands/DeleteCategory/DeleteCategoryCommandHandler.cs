using MediatR;
using ListingService.Application.Contracts.Persistence;

namespace ListingService.Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var exists = await _categoryRepository.ExistsByIdAsync(request.Id, cancellationToken);
        
        if (!exists)
        {
            return false;
        }
        
        await _categoryRepository.DeleteAsync(request.Id, cancellationToken);
        return true;
    }
}