using Annium.Data.Models;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Configuration interface for entities with created time tracking.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="ICreatedTimeEntity"/>.</typeparam>
public interface ICreatedTimeEntityConfiguration<TEntity> : IEntityConfiguration<TEntity>
    where TEntity : class, ICreatedTimeEntity;

/// <summary>
/// Configuration interface for entities with both created and updated time tracking.
/// </summary>
/// <typeparam name="TEntity">The entity type that implements <see cref="ICreatedUpdatedTimeEntity"/>.</typeparam>
public interface ICreatedUpdatedTimeEntityConfiguration<TEntity> : ICreatedTimeEntityConfiguration<TEntity>
    where TEntity : class, ICreatedUpdatedTimeEntity;

/// <summary>
/// Extension methods for configuring timed entity mappings.
/// </summary>
public static class TimedEntityConfigurationExtensions
{
    /// <summary>
    /// Configures auto-managed created time for an entity, where the database automatically sets the created time.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements <see cref="ICreatedTimeEntity"/>.</typeparam>
    /// <param name="configuration">The entity configuration.</param>
    /// <param name="builder">The entity mapping builder.</param>
    public static void ConfigureAutoCreatedTime<TEntity>(
        this ICreatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedTimeEntity
    {
        builder.Property(x => x.CreatedAt).IsColumn();
    }

    /// <summary>
    /// Configures auto-managed created and updated time for an entity, where the database automatically sets both timestamps.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements <see cref="ICreatedUpdatedTimeEntity"/>.</typeparam>
    /// <param name="configuration">The entity configuration.</param>
    /// <param name="builder">The entity mapping builder.</param>
    public static void ConfigureAutoCreatedUpdatedTime<TEntity>(
        this ICreatedUpdatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedUpdatedTimeEntity
    {
        configuration.ConfigureAutoCreatedTime(builder);
        builder.Property(x => x.UpdatedAt).IsColumn();
    }

    /// <summary>
    /// Configures manual created time for an entity, where the application manages the created time and skips it during insert/update operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements <see cref="ICreatedTimeEntity"/>.</typeparam>
    /// <param name="configuration">The entity configuration.</param>
    /// <param name="builder">The entity mapping builder.</param>
    public static void ConfigureManualCreatedTime<TEntity>(
        this ICreatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedTimeEntity
    {
        builder.Property(x => x.CreatedAt).IsColumn();
        builder.HasSkipValuesOnInsert(x => x.CreatedAt);
        builder.HasSkipValuesOnUpdate(x => x.CreatedAt);
    }

    /// <summary>
    /// Configures manual created and updated time for an entity, where the application manages both timestamps and skips them during insert/update operations.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that implements <see cref="ICreatedUpdatedTimeEntity"/>.</typeparam>
    /// <param name="configuration">The entity configuration.</param>
    /// <param name="builder">The entity mapping builder.</param>
    public static void ConfigureManualCreatedUpdatedTime<TEntity>(
        this ICreatedUpdatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedUpdatedTimeEntity
    {
        configuration.ConfigureManualCreatedTime(builder);
        builder.Property(x => x.UpdatedAt).IsColumn();
        builder.HasSkipValuesOnInsert(x => x.UpdatedAt);
        builder.HasSkipValuesOnUpdate(x => x.UpdatedAt);
    }
}
