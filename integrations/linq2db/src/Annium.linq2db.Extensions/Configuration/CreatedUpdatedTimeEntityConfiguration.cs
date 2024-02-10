using Annium.Data.Models;
using LinqToDB.Mapping;
using NodaTime;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public interface ICreatedTimeEntityConfiguration<TEntity> : IEntityConfiguration<TEntity>
    where TEntity : class, ICreatedTimeEntity;

public interface ICreatedUpdatedTimeEntityConfiguration<TEntity> : ICreatedTimeEntityConfiguration<TEntity>
    where TEntity : class, ICreatedUpdatedTimeEntity;

public static class TimedEntityConfigurationExtensions
{
    public static void ConfigureCreatedUpdatedTime<TEntity>(
        this ICreatedUpdatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedUpdatedTimeEntity
    {
        configuration.ConfigureCreatedTime(builder);
        builder.Property(x => x.UpdatedAt).IsColumn().HasConversionFunc(SetUpdatedDate, GetDate);
    }

    public static void ConfigureCreatedTime<TEntity>(
        this ICreatedTimeEntityConfiguration<TEntity> configuration,
        EntityMappingBuilder<TEntity> builder
    )
        where TEntity : class, ICreatedTimeEntity
    {
        builder.Property(x => x.CreatedAt).IsColumn().HasConversionFunc(SetCreatedDate, GetDate);
    }

    private static Instant SetCreatedDate(Instant value)
    {
        return value == default ? SystemClock.Instance.GetCurrentInstant() : value;
    }

    private static Instant SetUpdatedDate(Instant value)
    {
        return SystemClock.Instance.GetCurrentInstant();
    }

    private static Instant GetDate(Instant value)
    {
        return value == default ? SystemClock.Instance.GetCurrentInstant() : value;
    }
}
