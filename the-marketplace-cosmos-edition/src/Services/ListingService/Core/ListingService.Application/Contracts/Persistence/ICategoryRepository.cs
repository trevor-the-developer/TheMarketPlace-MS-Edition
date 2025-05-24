using ListingService.Domain.Entities;
using Services.Core.Models;

namespace ListingService.Application.Contracts.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BasePagedResult<Category>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<List<Category>> GetSubcategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}