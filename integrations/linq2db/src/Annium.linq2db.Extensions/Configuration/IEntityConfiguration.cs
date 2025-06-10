using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration;

public interface IEntityConfiguration<TEntity> : IEntityConfiguration
    where TEntity : class
{
    void Configure(EntityMappingBuilder<TEntity> builder);
}

public interface IEntityConfiguration;
