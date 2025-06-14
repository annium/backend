using System;
using System.Collections.Generic;
using System.Reflection;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

/// <summary>
/// Represents metadata information for a database table including its schema, name, and column information.
/// </summary>
public class TableMetadata
{
    /// <summary>
    /// Gets the schema name of the table, if specified.
    /// </summary>
    public string? Schema => Attribute.Schema;

    /// <summary>
    /// Gets the name of the table, using the attribute name if specified, otherwise the type name.
    /// </summary>
    public string Name => Attribute.Name ?? Type.Name;

    /// <summary>
    /// Gets the .NET entity type that maps to this table.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the table attribute that defines the table mapping.
    /// </summary>
    public TableAttribute Attribute { get; }

    /// <summary>
    /// Gets a dictionary of column metadata indexed by the member information.
    /// </summary>
    public IReadOnlyDictionary<MemberInfo, ColumnMetadata> Columns { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableMetadata"/> class.
    /// </summary>
    /// <param name="type">The .NET entity type that maps to this table.</param>
    /// <param name="attribute">The table attribute that defines the table mapping.</param>
    /// <param name="columns">A dictionary of column metadata indexed by member information.</param>
    public TableMetadata(Type type, TableAttribute attribute, IReadOnlyDictionary<MemberInfo, ColumnMetadata> columns)
    {
        Type = type;
        Attribute = attribute;
        Columns = columns;
    }

    /// <summary>
    /// Returns a string representation of the table metadata.
    /// </summary>
    /// <returns>The name of the table.</returns>
    public override string ToString() => Name;
}
