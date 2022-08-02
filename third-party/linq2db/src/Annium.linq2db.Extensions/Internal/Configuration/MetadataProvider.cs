using System;
using System.Linq;
using System.Reflection;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Metadata;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Configuration;

internal static class MetadataProvider
{
    public static DatabaseMetadata Describe(MappingSchema schema, MetadataFlags flags)
    {
        var types = schema.GetDefinedTypes();

        var tables = types.OrderBy(x => x.Name).Select(x => Build(schema, x, flags)).ToArray();

        return new DatabaseMetadata(tables);
    }

    private static TableMetadata Build(MappingSchema schema, Type type, MetadataFlags flags)
    {
        var table = schema.GetAttribute<TableAttribute>(type)!;

        var columns = type.GetProperties().Concat<MemberInfo>(type.GetFields())
            .Select(x => Build(schema, type, x, flags)!)
            .Where(x => x != null)
            .ToArray();

        return new TableMetadata(type, table, columns);
    }

    private static ColumnMetadata? Build(MappingSchema schema, Type type, MemberInfo member, MetadataFlags flags)
    {
        var memberType = member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field       => field.FieldType,
            _                     => throw new NotImplementedException($"Member {member} is not supported")
        };
        var column = schema.GetAttribute<ColumnAttribute>(type, member);

        // if not marked as column
        if (column is null)
        {
            // if requested to be included in metadata - add ColumnAttribute; otherwise - skip
            if (flags.HasFlag(MetadataFlags.IncludeMembersNotMarkedAsColumns))
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

        return new ColumnMetadata(member, memberType, column, dataType, nullable, primaryKey, association);
    }
}