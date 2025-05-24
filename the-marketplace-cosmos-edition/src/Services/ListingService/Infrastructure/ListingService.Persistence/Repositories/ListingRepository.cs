using Microsoft.EntityFrameworkCore;
using Services.Core.Models;
using ListingService.Application.Contracts.Persistence;
using ListingService.Domain.Entities;
using ListingService.Persistence.DatabaseContext;

namespace ListingService.Persistence.Repositories;

public class ListingRepository(ListingDatabaseContext context) : IListingRepository
{
    private readonly ListingDatabaseContext _context = context;

    public async Task<Listing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Tags)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<BasePagedResult<Listing>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Tags)
            .Include(l => l.Images)
            .AsQueryable();
            
        var totalCount = await query.CountAsync(cancellationToken);
        
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return new BasePagedResult<Listing>
        {
            Data = listings,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<BasePagedResult<Listing>> GetByCategoryIdAsync(Guid categoryId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Tags)
            .Include(l => l.Images)
            .Where(l => l.CategoryId == categoryId)
            .AsQueryable();
            
        var totalCount = await query.CountAsync(cancellationToken);
        
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return new BasePagedResult<Listing>
        {
            Data = listings,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<BasePagedResult<Listing>> GetBySellerIdAsync(Guid sellerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Tags)
            .Include(l => l.Images)
            .Where(l => l.SellerId == sellerId)
            .AsQueryable();
            
        var totalCount = await query.CountAsync(cancellationToken);
        
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return new BasePagedResult<Listing>
        {
            Data = listings,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<Listing> AddAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        await _context.Listings.AddAsync(listing, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return listing;
    }

    public async Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        _context.Entry(listing).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings.FindAsync(new object[] { id }, cancellationToken);
        if (listing != null)
        {
            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Listings.AnyAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<Listing>> GetRecentListingsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Tags)
            .Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Published)
            .OrderByDescending(l => l.PublishedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Listing>> GetFeaturedListingsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .Include(l => l.Category)
            .Include(l => l.Tags)
            .Include(l => l.Images)
            .Where(l => l.Status == ListingStatus.Published)
            .OrderByDescending(l => l.Views)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}