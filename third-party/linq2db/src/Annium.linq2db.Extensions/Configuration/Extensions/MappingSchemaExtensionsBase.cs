using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using Annium.linq2db.Extensions.Configuration.Metadata;
using Annium.linq2db.Extensions.Internal.Configuration;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration.Extensions;

public static class MappingSchemaExtensionsBase
{
    private readonly record struct DescriptionCacheKey(MappingSchema Schema, MetadataFlags Flags);

    private static readonly ConcurrentDictionary<DescriptionCacheKey, DatabaseMetadata> DatabaseMetadataCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IMappingBuilder GetMappingBuilder(
        this MappingSchema schema,
        Assembly configurationsAssembly
    ) => new MappingBuilder(configurationsAssembly, schema);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DatabaseMetadata Describe(
        this MappingSchema schema,
        MetadataFlags flags = MetadataFlags.None
    ) => MetadataProvider.Describe(schema, flags);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DatabaseMetadata CachedDescribe(
        this MappingSchema schema,
        MetadataFlags flags = MetadataFlags.None
    ) => DatabaseMetadataCache.GetOrAdd(
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
}