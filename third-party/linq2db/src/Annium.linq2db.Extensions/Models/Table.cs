using System;
using System.Collections.Generic;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Models;

public class Table
{
    public string? Schema => Attribute.Schema;
    public string Name => Attribute.Name ?? Type.Name;
    public Type Type { get; }
    public TableAttribute Attribute { get; }
    public IReadOnlyCollection<TableColumn> Columns { get; }

    public Table(
        Type type,
        TableAttribute attribute,
        IReadOnlyCollection<TableColumn> columns
    )
    {
        Type = type;
        Attribute = attribute;
        Columns = columns;
    }

    public override string ToString() => Name;
}