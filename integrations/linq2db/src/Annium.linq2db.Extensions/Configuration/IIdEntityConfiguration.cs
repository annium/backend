using Annium.Data.Models;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public interface IIdEntityConfiguration<TEntity, TId> : IEntityConfiguration<TEntity>
    where TEntity : class, IIdEntity<TId>
    where TId : struct;

public static class IdEntityConfigurationExtensions
{
    public static void ConfigureId<TEntity, TId>(
        this IIdEntityConfiguration<TEntity, TId> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, IIdEntity<TId>
        where TId : struct
    {
        builder.HasPrimaryKey(x => x.Id);
        builder.Property(x => x.Id).IsColumn();
    }
}
