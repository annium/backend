using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Annium.Core.DependencyInjection;
using Annium.linq2db.Extensions.Internal;
using Annium.Reflection;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public static class MappingSchemaExtensionsBase
{
    private readonly record struct DescriptionCacheKey(MappingSchema Schema, MetadataFlags Flags);

    private static readonly ConcurrentDictionary<DescriptionCacheKey, DatabaseMetadata> DatabaseMetadataCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DatabaseMetadata Describe(this MappingSchema schema, MetadataFlags flags = MetadataFlags.None) =>
        MetadataProvider.Describe(schema, flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DatabaseMetadata CachedDescribe(
        this MappingSchema schema,
        MetadataFlags flags = MetadataFlags.None
    ) =>
        DatabaseMetadataCache.GetOrAdd(
            new DescriptionCacheKey(schema, flags),
            key => MetadataProvider.Describe(key.Schema, key.Flags)
        );

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
