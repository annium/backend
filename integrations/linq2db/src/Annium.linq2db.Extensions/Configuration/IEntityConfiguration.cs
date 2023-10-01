using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public interface IEntityConfiguration<TEntity> : IEntityConfiguration
    where TEntity : class
{
    void Configure(EntityMappingBuilder<TEntity> builder);
}

public interface IEntityConfiguration
{
}