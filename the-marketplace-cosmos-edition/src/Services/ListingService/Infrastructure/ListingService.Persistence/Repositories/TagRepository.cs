using Microsoft.EntityFrameworkCore;
using Services.Core.Models;
using ListingService.Application.Contracts.Persistence;
using ListingService.Domain.Entities;
using ListingService.Persistence.DatabaseContext;

namespace ListingService.Persistence.Repositories;

public class TagRepository : ITagRepository
{
    private readonly ListingDatabaseContext _context;

    public TagRepository(ListingDatabaseContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<BasePagedResult<Tag>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Tags.AsQueryable();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var tags = await query
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
            
        return new BasePagedResult<Tag>
        {
            Data = tags,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Include(t => t.Listings)
            .OrderByDescending(t => t.Listings != null ? t.Listings.Count : 0)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Tag>> GetByListingIdAsync(Guid listingId, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .Where(t => t.Listings != null && t.Listings.Any(l => l.Id == listingId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Tag> AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await _context.Tags.AddAsync(tag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return tag;
    }

    public async Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _context.Entry(tag).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _context.Tags.FindAsync(new object[] { id }, cancellationToken);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.AnyAsync(t => t.Id == id, cancellationToken);
    }
}