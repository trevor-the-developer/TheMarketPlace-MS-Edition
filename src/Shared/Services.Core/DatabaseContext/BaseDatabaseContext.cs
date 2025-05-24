using Microsoft.EntityFrameworkCore;
using Services.Core.Entities;

namespace Services.Core.DatabaseContext;

public abstract class BaseDatabaseContext<T>(DbContextOptions<T> options) : DbContext(options) where T : DbContext
{
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var timeStamp = DateTime.UtcNow;
        
        foreach (var entry in base.ChangeTracker.Entries<BaseEntity>()
                     .Where(q => q.State is EntityState.Added or EntityState.Modified))
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = timeStamp;
                    entry.Entity.UpdatedAt = timeStamp;
                    break;
                
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = timeStamp;
                    entry.Property(q => q.UpdatedAt).IsModified = false;
                    break;
            }
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}