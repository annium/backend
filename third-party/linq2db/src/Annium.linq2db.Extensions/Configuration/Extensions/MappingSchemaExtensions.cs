using System.Reflection;
using Annium.linq2db.Extensions.Internal.Configuration;
using LinqToDB.Mapping;

namespace Annium.linq2db.Extensions.Configuration.Extensions;

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