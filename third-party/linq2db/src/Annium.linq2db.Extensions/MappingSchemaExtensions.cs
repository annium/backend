using System.Reflection;
using Annium.linq2db.Extensions.Internal;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions;

public static class MappingSchemaExtensions
{
    public static IMappingBuilder GetMappingBuilder(
        this MappingSchema mappingSchema,
        Assembly configurationsAssembly
    )
    {
        return new MappingBuilder(configurationsAssembly, mappingSchema);
    }
}