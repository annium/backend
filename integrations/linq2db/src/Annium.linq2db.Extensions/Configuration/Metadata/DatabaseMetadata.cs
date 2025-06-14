using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Represents metadata information for a database, containing information about all mapped tables.
/// </summary>
public class DatabaseMetadata
{
    /// <summary>
    /// Gets a dictionary of table metadata indexed by the entity type.
    /// </summary>
    public IReadOnlyDictionary<Type, TableMetadata> Tables { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseMetadata"/> class.
    /// </summary>
    /// <param name="tables">A dictionary of table metadata indexed by entity type.</param>
    public DatabaseMetadata(IReadOnlyDictionary<Type, TableMetadata> tables)
    {
        Tables = tables;
    }
}
