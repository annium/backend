using System;
using System.Reflection;
using LinqToDB.Extensions;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration.Metadata;

/// <summary>
/// Represents metadata information for a database column including its mapping attributes and type information.
/// </summary>
public class ColumnMetadata
{
    /// <summary>
    /// Gets the name of the column, using the attribute name if specified, otherwise the member name.
    /// </summary>
    public string Name => Attribute.Name ?? Member.Name;

    /// <summary>
    /// Gets the member information (property or field) that this column maps to.
    /// </summary>
    public MemberInfo Member { get; }

    /// <summary>
    /// Gets the .NET type of the column.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the column attribute that defines the column mapping.
    /// </summary>
    public ColumnAttribute Attribute { get; }

    /// <summary>
    /// Gets the data type attribute if specified, which defines the database data type.
    /// </summary>
    public DataTypeAttribute? DataType { get; }

    /// <summary>
    /// Gets the nullable attribute if specified, which defines if the column can contain null values.
    /// </summary>
    public NullableAttribute? Nullable { get; }

    /// <summary>
    /// Gets the primary key attribute if specified, which defines if the column is part of the primary key.
    /// </summary>
    public PrimaryKeyAttribute? PrimaryKey { get; }

    /// <summary>
    /// Gets the association attribute if specified, which defines foreign key relationships.
    /// </summary>
    public AssociationAttribute? Association { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnMetadata"/> class.
    /// </summary>
    /// <param name="member">The member information (property or field) that this column maps to.</param>
    /// <param name="type">The .NET type of the column.</param>
    /// <param name="attribute">The column attribute that defines the column mapping.</param>
    /// <param name="dataType">The data type attribute if specified.</param>
    /// <param name="nullable">The nullable attribute if specified.</param>
    /// <param name="primaryKey">The primary key attribute if specified.</param>
    /// <param name="association">The association attribute if specified.</param>
    public ColumnMetadata(
        MemberInfo member,
        Type type,
        ColumnAttribute attribute,
        DataTypeAttribute? dataType,
        NullableAttribute? nullable,
        PrimaryKeyAttribute? primaryKey,
        AssociationAttribute? association
    )
    {
        Member = member;
        Type = type;
        Attribute = attribute;
        DataType = dataType;
        Nullable = nullable;
        PrimaryKey = primaryKey;
        Association = association;
    }

    /// <summary>
    /// Returns a string representation of the column metadata.
    /// </summary>
    /// <returns>A string containing the member type name and column name.</returns>
    public override string ToString() => $"{Member.GetMemberType().Name} {Name}";
}
