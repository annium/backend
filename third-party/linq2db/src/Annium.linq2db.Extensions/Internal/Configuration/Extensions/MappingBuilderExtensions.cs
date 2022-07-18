using System.Linq;
using Annium.linq2db.Extensions.Configuration;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Internal.Configuration.Extensions;

internal static class MappingBuilderExtensions
{
    public static IMappingBuilder IncludeAssociationKeysAsColumns(this IMappingBuilder builder) => builder.Configure(db =>
    {
        var fluentBuilder = builder.Map;

        foreach (var table in db.Tables)
        foreach (var column in table.Columns)
        {
            if (column.Attribute.IsColumn)
                continue;

            // check if is foreign key
            var isForeignKey = table.Columns
                .Where(x => x.Attribute.IsColumn)
                .Any(x => x.Association?.ThisKey == column.Member.Name);

            // set as basic column
            if (isForeignKey)
                fluentBuilder.HasAttribute(column.Member, new ColumnAttribute { IsColumn = true });
        }
    }, MetadataBuilderFlags.IncludeMembersNotMarkedAsColumns);
}