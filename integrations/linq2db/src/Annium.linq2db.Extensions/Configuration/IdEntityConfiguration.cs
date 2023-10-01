using Annium.Data.Models;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public abstract class IdEntityConfiguration<TEntity, TId> : IEntityConfiguration<TEntity>
    where TEntity : class, IIdEntity<TId>
    where TId : struct
{
    public virtual void Configure(EntityMappingBuilder<TEntity> builder)
    {
        builder.HasPrimaryKey(x => x.Id);
        builder.Property(x => x.Id).IsColumn();
    }
}