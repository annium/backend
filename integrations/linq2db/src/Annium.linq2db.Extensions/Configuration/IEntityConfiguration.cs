using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Provides entity configuration for linq2db mapping
/// </summary>
/// <typeparam name="TEntity">The entity type to configure</typeparam>
public interface IEntityConfiguration<TEntity> : IEntityConfiguration
    where TEntity : class
{
    /// <summary>
    /// Configures the mapping for the entity
    /// </summary>
    /// <param name="builder">The entity mapping builder</param>
    void Configure(EntityMappingBuilder<TEntity> builder);
}

/// <summary>
/// Base interface for entity configurations
/// </summary>
public interface IEntityConfiguration;
