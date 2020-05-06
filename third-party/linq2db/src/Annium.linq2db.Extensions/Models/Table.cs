using System;
using System.Collections.Generic;

namespace Annium.linq2db.Extensions.Models
{
    public class Table
    {
        public Type Type { get; }
        public string? Schema { get; }
        public string Name { get; }
        public IReadOnlyCollection<TableColumn> Columns { get; }

        public Table(
            Type type,
            string? schema,
            string name,
            IReadOnlyCollection<TableColumn> columns
        )
        {
            Type = type;
            Schema = schema;
            Name = name;
            Columns = columns;
        }

        public override string ToString() => Name;
    }
}