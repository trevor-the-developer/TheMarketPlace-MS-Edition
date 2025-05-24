using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Services.Core.Entities;

namespace Services.Core.ConfigurationExtensions;

public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(entity => entity.CreatedAt)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAt)
            .IsRequired();
    }

    protected abstract void ConfigureTable(EntityTypeBuilder<T> builder);

    protected abstract void ConfigureProperties(EntityTypeBuilder<T> builder);

    protected virtual void ConfigureRelationships(EntityTypeBuilder<T> builder) { }
    
    protected virtual void ConfigureSeedData(EntityTypeBuilder<T> builder) { }
}