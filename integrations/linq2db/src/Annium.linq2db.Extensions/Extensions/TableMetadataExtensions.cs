using System;
using Annium.linq2db.Extensions.Configuration.Extensions;
using Annium.linq2db.Extensions.Configuration.Metadata;
using LinqToDB;

namespace Annium.linq2db.Extensions.Extensions;

/// <summary>
/// Extension methods for ITable to retrieve metadata information.
/// </summary>
public static class TableMetadataExtensions
{
    /// <summary>
    /// Gets the table metadata for the specified table type from the mapping schema.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="table">The table to get metadata for.</param>
    /// <returns>The table metadata containing information about columns, attributes, and mapping.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the entity type is not found in the database metadata.</exception>
    public static TableMetadata GetMetadata<T>(this ITable<T> table)
        where T : notnull =>
        table.DataContext.MappingSchema.CachedDescribe().Tables.TryGetValue(typeof(T), out var tableMetadata)
            ? tableMetadata
            : throw new InvalidOperationException($"Type {typeof(T).FriendlyName()} is missing in database metadata");
}
