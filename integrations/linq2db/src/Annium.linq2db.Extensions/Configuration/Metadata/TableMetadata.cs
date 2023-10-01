using System;
using System.Collections.Generic;
using System.Reflection;
using LinqToDB.Mapping;

// ReSharper disable once CheckNamespace
namespace Annium.linq2db.Extensions;

public class TableMetadata
{
    public string? Schema => Attribute.Schema;
    public string Name => Attribute.Name ?? Type.Name;
    public Type Type { get; }
    public TableAttribute Attribute { get; }
    public IReadOnlyDictionary<MemberInfo, ColumnMetadata> Columns { get; }

    public TableMetadata(
        Type type,
        TableAttribute attribute,
        IReadOnlyDictionary<MemberInfo, ColumnMetadata> columns
    )
    {
        Type = type;
        Attribute = attribute;
        Columns = columns;
    }

    public override string ToString() => Name;
}