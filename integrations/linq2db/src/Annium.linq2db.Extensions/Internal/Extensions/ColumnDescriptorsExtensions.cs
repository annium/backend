using System.Collections.Generic;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Extensions;

internal static class ColumnDescriptorsExtensions
{
    /// <summary>
    /// Finds a column descriptor by name that doesn't have skip values configured.
    /// </summary>
    /// <param name="columns">The collection of column descriptors.</param>
    /// <param name="name">The column name to find.</param>
    /// <returns>The matching column descriptor (throws if not found).</returns>
    public static ColumnDescriptor FindColumn(this IEnumerable<ColumnDescriptor> columns, string name)
    {
        return columns.TryFindColumn(name).NotNull();
    }

    /// <summary>
    /// Tries to finds a column descriptor by name that doesn't have skip values configured.
    /// </summary>
    /// <param name="columns">The collection of column descriptors.</param>
    /// <param name="name">The column name to find.</param>
    /// <returns>The matching column descriptor or null if not found.</returns>
    public static ColumnDescriptor? TryFindColumn(this IEnumerable<ColumnDescriptor> columns, string name)
    {
        foreach (var column in columns)
            if (
                column.MemberAccessor.MemberInfo.Name == name
                && column is { HasValuesToSkipOnInsert: false, HasValuesToSkipOnUpdate: false }
            )
                return column;

        return null;
    }
}
