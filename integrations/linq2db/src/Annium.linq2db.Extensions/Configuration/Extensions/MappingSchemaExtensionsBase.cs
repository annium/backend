using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Core.DependencyInjection;
using Annium.linq2db.Extensions.Configuration.Metadata;
using Annium.linq2db.Extensions.Internal.Configuration;
using Annium.linq2db.Extensions.Internal.Configuration.Extensions;
using Annium.Reflection;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration.Extensions;

/// <summary>
/// Base extension methods for MappingSchema providing core functionality for metadata description and configuration application.
/// </summary>
public static class MappingSchemaExtensionsBase
{
    /// <summary>
    /// Cache key for database metadata descriptions.
    /// </summary>
    /// <param name="Schema">The mapping schema.</param>
    /// <param name="Flags">The metadata flags.</param>
    private readonly record struct DescriptionCacheKey(MappingSchema Schema, MetadataFlags Flags);

    /// <summary>
    /// Thread-safe cache for database metadata descriptions to avoid repeated analysis of mapping schemas
    /// </summary>
    private static readonly ConcurrentDictionary<DescriptionCacheKey, DatabaseMetadata> _databaseMetadataCache = new();

    /// <summary>
    /// Creates a database metadata description from the mapping schema.
    /// </summary>
    /// <param name="schema">The mapping schema to describe.</param>
    /// <param name="flags">The metadata flags to control what metadata is included.</param>
    /// <returns>The database metadata description.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DatabaseMetadata Describe(this MappingSchema schema, MetadataFlags flags = MetadataFlags.None) =>
        MetadataProvider.Describe(schema, flags);

    /// <summary>
    /// Creates a cached database metadata description from the mapping schema.
    /// </summary>
    /// <param name="schema">The mapping schema to describe.</param>
    /// <param name="flags">The metadata flags to control what metadata is included.</param>
    /// <returns>The cached database metadata description.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DatabaseMetadata CachedDescribe(
        this MappingSchema schema,
        MetadataFlags flags = MetadataFlags.None
    ) =>
        _databaseMetadataCache.GetOrAdd(
            new DescriptionCacheKey(schema, flags),
            key => MetadataProvider.Describe(key.Schema, key.Flags)
        );

    /// <summary>
    /// Configures the mapping schema by providing access to its database metadata.
    /// </summary>
    /// <param name="schema">The mapping schema to configure.</param>
    /// <param name="configure">The configuration action that receives the database metadata.</param>
    /// <param name="flags">The metadata flags to control what metadata is included.</param>
    /// <returns>The configured mapping schema.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MappingSchema Configure(
        this MappingSchema schema,
        Action<DatabaseMetadata> configure,
        MetadataFlags flags = MetadataFlags.None
    )
    {
        configure(schema.Describe(flags));

        return schema;
    }

    /// <summary>
    /// Applies all registered entity configurations from the service provider to the mapping schema.
    /// </summary>
    /// <param name="schema">The mapping schema to configure.</param>
    /// <param name="sp">The service provider containing entity configurations.</param>
    /// <returns>The configured mapping schema with all entity configurations applied.</returns>
    public static MappingSchema ApplyConfigurations(this MappingSchema schema, IServiceProvider sp)
    {
        var entityMappingBuilderFactory = typeof(FluentMappingBuilder).GetMethod(nameof(FluentMappingBuilder.Entity))!;
        var mappingBuilder = new FluentMappingBuilder(schema);

        var configurations = sp.Resolve<IEnumerable<IEntityConfiguration>>().ToImmutableHashSet();
        foreach (var configuration in configurations)
        {
            var configurationType = configuration.GetType().GetTargetImplementation(typeof(IEntityConfiguration<>));
            if (configurationType is null)
                throw new InvalidOperationException(
                    $"Configuration {configuration} doesn't implement {typeof(IEntityConfiguration<>).FriendlyName()}"
                );

            var entityType = configurationType.GenericTypeArguments.Single();
            var entityMappingBuilder = entityMappingBuilderFactory
                .MakeGenericMethod(entityType)
                .Invoke(mappingBuilder, new object?[] { null })!;
            var configureMethod = typeof(IEntityConfiguration<>)
                .MakeGenericType(entityType)
                .GetMethod(nameof(IEntityConfiguration<object>.Configure))!;
            configureMethod.Invoke(configuration, new[] { entityMappingBuilder });
        }

        mappingBuilder.Build();

        schema.IncludeAssociationKeysAsColumns();
        schema.MarkNotColumnsExplicitly();

        return schema;
    }
}
