using System;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Configuration.Metadata;
using LinqToDB;

namespace Annium.linq2db.Extensions.Extensions;

public static class TableMetadataExtensions
{
    public static TableMetadata GetMetadata<T>(this ITable<T> table)
        where T : notnull
        => table.DataContext.MappingSchema.CachedDescribe().Tables.TryGetValue(typeof(T), out var tableMetadata)
            ? tableMetadata
            : throw new InvalidOperationException($"Type {typeof(T).FriendlyName()} is missing in database metadata");
}