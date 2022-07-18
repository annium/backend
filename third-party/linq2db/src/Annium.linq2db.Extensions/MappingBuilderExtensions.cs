using Annium.Core.Primitives;

namespace Annium.linq2db.Extensions;

public static class MappingBuilderExtensions
{
    public static IMappingBuilder SnakeCaseColumns(this IMappingBuilder builder) => builder.Configure(db =>
    {
        foreach (var table in db.Tables)
        foreach (var column in table.Columns)
            column.Attribute.Name = column.Member.Name.SnakeCase();
    }, MetadataBuilderFlags.IncludeMembersNotMarkedAsColumns);
}