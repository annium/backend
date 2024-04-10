using Annium.Data.Models;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public interface ICreatedTimeEntityConfiguration<TEntity> : IEntityConfiguration<TEntity>
    where TEntity : class, ICreatedTimeEntity;

public interface ICreatedUpdatedTimeEntityConfiguration<TEntity> : ICreatedTimeEntityConfiguration<TEntity>
    where TEntity : class, ICreatedUpdatedTimeEntity;

public static class TimedEntityConfigurationExtensions
{
    public static void ConfigureAutoCreatedTime<TEntity>(
        this ICreatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedTimeEntity
    {
        builder.Property(x => x.CreatedAt).IsColumn();
    }

    public static void ConfigureAutoCreatedUpdatedTime<TEntity>(
        this ICreatedUpdatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedUpdatedTimeEntity
    {
        configuration.ConfigureAutoCreatedTime(builder);
        builder.Property(x => x.UpdatedAt).IsColumn();
    }

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