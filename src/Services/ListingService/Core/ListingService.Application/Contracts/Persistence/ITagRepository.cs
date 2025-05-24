using ListingService.Domain.Entities;
using Services.Core.Models;

namespace ListingService.Application.Contracts.Persistence;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<BasePagedResult<Tag>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default);
    Task<List<Tag>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default);
    Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}