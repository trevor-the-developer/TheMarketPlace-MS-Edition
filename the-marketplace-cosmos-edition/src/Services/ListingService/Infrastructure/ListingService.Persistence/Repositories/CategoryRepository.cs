using Microsoft.EntityFrameworkCore;
using Services.Core.Models;
using ListingService.Application.Contracts.Persistence;
using ListingService.Domain.Entities;
using ListingService.Persistence.DatabaseContext;

namespace ListingService.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ListingDatabaseContext _context;

    public CategoryRepository(ListingDatabaseContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<BasePagedResult<Category>> GetAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var categories = await query
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new BasePagedResult<Category>
        {
            Data = categories,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Category>> GetSubcategoriesAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Entry(category).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { id }, cancellationToken);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id, cancellationToken);
    }
}