using System;
using System.Linq;
using System.Reflection;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    internal class MetadataBuilder
    {
        public Database Build(MappingSchema schema)
        {
            var types = schema.GetDefinedTypes();

            var tables = types.Select(x => Build(schema, x)).ToArray();

            return new Database(tables);
        }

        public Table Build(MappingSchema schema, Type type)
        {
            var table = schema.GetAttribute<TableAttribute>(type)!;

            var columns = type.GetProperties().Concat<MemberInfo>(type.GetFields())
                .Select(x => Build(schema, type, x)!)
                .Where(x => x != null)
                .ToArray();

            return new Table(type, table, columns);
        }

        public TableColumn? Build(MappingSchema schema, Type type, MemberInfo member)
        {
            var column = schema.GetAttribute<ColumnAttribute>(type, member);
            if (column is null || !column.IsColumn)
                return null;

            var dataType = schema.GetAttribute<DataTypeAttribute>(type, member);
            var nullable = schema.GetAttribute<NullableAttribute>(type, member);
            var association = schema.GetAttribute<AssociationAttribute>(type, member);

            return new TableColumn(member, column, dataType, nullable, association);
        }
    }
}