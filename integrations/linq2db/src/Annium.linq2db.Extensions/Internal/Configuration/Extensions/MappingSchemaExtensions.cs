using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Configuration.Extensions;

/// <summary>
/// Internal extension methods for configuring MappingSchema instances with additional metadata handling.
/// </summary>
internal static class MappingSchemaExtensions
{
    /// <summary>
    /// Configures the mapping schema to include association foreign key properties as columns.
    /// This method identifies properties that serve as foreign keys in associations and marks them as columns.
    /// </summary>
    /// <param name="schema">The mapping schema to configure</param>
    /// <returns>The configured mapping schema with association keys marked as columns</returns>
    public static MappingSchema IncludeAssociationKeysAsColumns(this MappingSchema schema) =>
        schema.Configure(
            db =>
            {
                var mappingBuilder = new FluentMappingBuilder(schema);

                foreach (var table in db.Tables.Values)
                foreach (var column in table.Columns.Values)
                {
                    if (column.Attribute.IsColumn)
                        continue;

                    // check if is foreign key
                    var isForeignKey = Enumerable
                        .Where<ColumnMetadata>(table.Columns.Values, x => x.Attribute.IsColumn)
                        .Any(x => x.Association?.ThisKey == column.Member.Name);

                    // set as basic column
                    if (isForeignKey)
                        mappingBuilder.HasAttribute((MemberInfo)column.Member, new ColumnAttribute { IsColumn = true });
                }

                mappingBuilder.Build();
            },
            MetadataFlags.IncludeMembersNotMarkedAsColumns
        );

    /// <summary>
    /// Configures the mapping schema to explicitly mark non-column properties with IsNotColumn.
    /// This method uses reflection to dynamically build property mappings for members that are not marked as columns.
    /// </summary>
    /// <param name="schema">The mapping schema to configure</param>
    /// <returns>The configured mapping schema with non-columns explicitly marked</returns>
    public static MappingSchema MarkNotColumnsExplicitly(this MappingSchema schema) =>
        schema.Configure(
            db =>
            {
                var entityMappingBuilderFactory = typeof(FluentMappingBuilder).GetMethod(
                    nameof(FluentMappingBuilder.Entity)
                )!;
                var mappingBuilder = new FluentMappingBuilder(schema);

                foreach (var table in db.Tables.Values)
                {
                    var entityMappingBuilder = entityMappingBuilderFactory
                        .MakeGenericMethod(table.Type)
                        .Invoke(mappingBuilder, new object?[] { null })!;
                    var getPropertyMappingBuilder = entityMappingBuilder
                        .GetType()
                        .GetMethod(nameof(EntityMappingBuilder<object>.Property))!;

                    foreach (var column in table.Columns.Values)
                    {
                        if (column.Attribute.IsColumn)
                            continue;

                        var typeParameter = Expression.Parameter(table.Type);
                        var propertyMappingBuilder = getPropertyMappingBuilder
                            .MakeGenericMethod(column.Type)
                            .Invoke(
                                entityMappingBuilder,
                                new object[]
                                {
                                    Expression.Lambda(
                                        Expression.PropertyOrField(typeParameter, column.Member.Name),
                                        typeParameter
                                    ),
                                }
                            )!;
                        var isNotColumn = propertyMappingBuilder
                            .GetType()
                            .GetMethod(nameof(PropertyMappingBuilder<object, object>.IsNotColumn))!;
                        isNotColumn.Invoke(propertyMappingBuilder, Array.Empty<object>());
                    }
                }

                mappingBuilder.Build();
            },
            MetadataFlags.IncludeMembersNotMarkedAsColumns
        );
}
