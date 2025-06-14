using System;
using System.Linq;
using System.Reflection;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Configuration;

/// <summary>
/// Internal provider for extracting database metadata from LinqToDB mapping schemas.
/// This class analyzes mapping schemas to build comprehensive metadata about tables and columns.
/// </summary>
internal static class MetadataProvider
{
    /// <summary>
    /// Binding flags used to discover all instance members (properties and fields) that may represent database columns
    /// </summary>
    private static readonly BindingFlags _columnMemberFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// Describes the database structure by analyzing the mapping schema and extracting metadata.
    /// </summary>
    /// <param name="schema">The mapping schema to analyze</param>
    /// <param name="flags">Flags controlling what metadata to include</param>
    /// <returns>Complete database metadata including all tables and their columns</returns>
    public static DatabaseMetadata Describe(MappingSchema schema, MetadataFlags flags)
    {
        var types = schema.GetDefinedTypes();

        var tables = types.ToDictionary(x => x, x => Build(schema, x, flags));

        return new DatabaseMetadata(tables);
    }

    /// <summary>
    /// Builds table metadata for a specific entity type by analyzing its properties and fields.
    /// </summary>
    /// <param name="schema">The mapping schema containing type configuration</param>
    /// <param name="type">The entity type to analyze</param>
    /// <param name="flags">Flags controlling what metadata to include</param>
    /// <returns>Complete table metadata including all columns</returns>
    private static TableMetadata Build(MappingSchema schema, Type type, MetadataFlags flags)
    {
        var table = schema.GetAttribute<TableAttribute>(type)!;

        var properties = type.GetProperties(_columnMemberFlags);
        var fields = type.GetFields(_columnMemberFlags);
        var members = properties.Concat<MemberInfo>(fields).ToArray();

        var columns = members
            .Select(x => Build(schema, type, x, flags))
            .OfType<ColumnMetadata>()
            .ToDictionary(x => x.Member);

        return new TableMetadata(type, table, columns);
    }

    /// <summary>
    /// Builds column metadata for a specific member (property or field) of an entity type.
    /// Analyzes all relevant attributes including column, data type, nullable, primary key, and association attributes.
    /// </summary>
    /// <param name="schema">The mapping schema containing member configuration</param>
    /// <param name="type">The entity type containing the member</param>
    /// <param name="member">The member to analyze (property or field)</param>
    /// <param name="flags">Flags controlling what metadata to include</param>
    /// <returns>Column metadata if the member represents a column, null otherwise</returns>
    private static ColumnMetadata? Build(MappingSchema schema, Type type, MemberInfo member, MetadataFlags flags)
    {
        var memberType = member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new InvalidOperationException($"Member {member} is not supported"),
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
