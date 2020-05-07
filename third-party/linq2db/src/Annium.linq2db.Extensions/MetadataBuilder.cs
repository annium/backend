using System;
using System.Linq;
using System.Reflection;
using Annium.linq2db.Extensions.Models;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    internal class MetadataBuilder
    {
        public Database Build(MappingSchema schema, MetadataBuilderFlags flags)
        {
            var types = schema.GetDefinedTypes();

            var tables = types.OrderBy(x => x.Name).Select(x => Build(schema, x, flags)).ToArray();

            return new Database(tables);
        }

        private Table Build(MappingSchema schema, Type type, MetadataBuilderFlags flags)
        {
            var table = schema.GetAttribute<TableAttribute>(type)!;

            var columns = type.GetProperties().Concat<MemberInfo>(type.GetFields())
                .Select(x => Build(schema, type, x, flags)!)
                .Where(x => x != null)
                .ToArray();

            return new Table(type, table, columns);
        }

        private TableColumn? Build(MappingSchema schema, Type type, MemberInfo member, MetadataBuilderFlags flags)
        {
            var column = schema.GetAttribute<ColumnAttribute>(type, member);

            // if not marked as column
            if (column is null)
            {
                // if requested to be included in metadata - add ColumnAttribute; otherwise - skip
                if (flags.HasFlag(MetadataBuilderFlags.IncludeMembersNotMarkedAsColumns))
                    column = new ColumnAttribute(member.Name) { IsColumn = false };
                else
                    return null;
            }
            // if explicitly not column (from schema, note code above) - skip
            else if (!column.IsColumn)
                return null;

            var dataType = schema.GetAttribute<DataTypeAttribute>(type, member);
            var nullable = schema.GetAttribute<NullableAttribute>(type, member);
            var primaryKey = schema.GetAttribute<PrimaryKeyAttribute>(type, member);
            var association = schema.GetAttribute<AssociationAttribute>(type, member);

            return new TableColumn(member, column, dataType, nullable, primaryKey, association);
        }
    }
}