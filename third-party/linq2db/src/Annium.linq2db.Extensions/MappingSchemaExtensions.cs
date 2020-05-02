using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    public static class MappingSchemaExtensions
    {
        public static MappingBuilder GetMappingBuilder(this MappingSchema mappingSchema)
        {
            return new MappingBuilder(mappingSchema.GetFluentMappingBuilder());
        }
    }
}