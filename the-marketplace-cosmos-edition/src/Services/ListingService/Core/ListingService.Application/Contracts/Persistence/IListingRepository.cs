using ListingService.Domain.Entities;
using Services.Core.Models;

namespace ListingService.Application.Contracts.Persistence;

public interface IListingRepository
{
    Task<Listing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BasePagedResult<Listing>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<BasePagedResult<Listing>> GetByCategoryIdAsync(Guid categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<BasePagedResult<Listing>> GetBySellerIdAsync(Guid sellerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Listing> AddAsync(Listing listing, CancellationToken cancellationToken = default);
    Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Listing>> GetRecentListingsAsync(int count, CancellationToken cancellationToken = default);
    Task<List<Listing>> GetFeaturedListingsAsync(int count, CancellationToken cancellationToken = default);
}