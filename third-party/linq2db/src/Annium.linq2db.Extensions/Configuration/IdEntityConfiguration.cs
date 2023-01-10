using Annium.Data.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration;

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