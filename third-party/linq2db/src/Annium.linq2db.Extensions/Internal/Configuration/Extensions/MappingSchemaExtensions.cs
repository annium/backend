using System.Linq;
using Annium.linq2db.Extensions.Configuration;
using Annium.linq2db.Extensions.Configuration.Extensions;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Configuration.Extensions;

internal static class MappingSchemaExtensions
{
    public static MappingSchema IncludeAssociationKeysAsColumns(this MappingSchema schema) => schema.Configure(db =>
    {
        var fluentBuilder = schema.GetFluentMappingBuilder();

        foreach (var table in db.Tables.Values)
        foreach (var column in table.Columns.Values)
        {
            if (column.Attribute.IsColumn)
                continue;

            // check if is foreign key
            var isForeignKey = table.Columns.Values
                .Where(x => x.Attribute.IsColumn)
                .Any(x => x.Association?.ThisKey == column.Member.Name);

            // set as basic column
            if (isForeignKey)
                fluentBuilder.HasAttribute(column.Member, new ColumnAttribute { IsColumn = true });
        }
    }, MetadataFlags.IncludeMembersNotMarkedAsColumns);
}