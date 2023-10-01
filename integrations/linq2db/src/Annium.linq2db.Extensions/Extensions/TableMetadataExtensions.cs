using System;
using LinqToDB;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public static class TableMetadataExtensions
{
    public static TableMetadata GetMetadata<T>(this ITable<T> table)
        where T : notnull
        => table.DataContext.MappingSchema.CachedDescribe().Tables.TryGetValue(typeof(T), out var tableMetadata)
            ? tableMetadata
            : throw new InvalidOperationException($"Type {typeof(T).FriendlyName()} is missing in database metadata");
}