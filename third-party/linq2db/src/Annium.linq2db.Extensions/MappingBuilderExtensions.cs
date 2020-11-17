using System.Linq;
using Annium.Core.Primitives;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    public static class MappingBuilderExtensions
    {
        internal static MappingBuilder IncludeAssociationKeysAsColumns(this MappingBuilder builder) => builder.Configure(db =>
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

        public static MappingBuilder SnakeCaseColumns(this MappingBuilder builder) => builder.Configure(db =>
        {
            foreach (var table in db.Tables)
            foreach (var column in table.Columns)
                column.Attribute.Name = column.Member.Name.SnakeCase();
        });
    }
}