using Annium.Data.Models;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Entity configuration interface for entities with ID properties
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TId">The ID property type</typeparam>
public interface IIdEntityConfiguration<TEntity, TId> : IEntityConfiguration<TEntity>
    where TEntity : class, IIdEntity<TId>
    where TId : struct;

/// <summary>
/// Extension methods for ID entity configurations
/// </summary>
public static class IdEntityConfigurationExtensions
{
    /// <summary>
    /// Configures the ID property as primary key for an entity
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TId">The ID property type</typeparam>
    /// <param name="configuration">The entity configuration</param>
    /// <param name="builder">The entity mapping builder</param>
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
