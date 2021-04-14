using System.Reflection;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions
{
    public static class MappingSchemaExtensions
    {
        public static MappingBuilder GetMappingBuilder(
            this MappingSchema mappingSchema,
            Assembly configurationsAssembly
        )
        {
            return new(configurationsAssembly, mappingSchema);
        }
    }
}