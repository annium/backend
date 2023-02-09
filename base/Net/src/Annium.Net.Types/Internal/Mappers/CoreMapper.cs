using Annium.Core.Internal;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class CoreMapper
{
    public static ITypeModel Map(ContextualType type, IMapperContext ctx) => Map(type, type.Nullability, ctx);

    private static ITypeModel Map(ContextualType type, Nullability nullability, IMapperContext ctx)
    {
        Log.Trace($"Map {type}");
        return NullableMapper.Map(type, nullability, ctx)
            ?? GenericParameterMapper.Map(type)
            ?? BaseTypeMapper.Map(type)
            ?? EnumMapper.Map(type)
            ?? SpecialMapper.Map(type, ctx)
            ?? ArrayMapper.Map(type, ctx)
            ?? RecordMapper.Map(type, ctx)
            ?? StructMapper.Map(type, ctx);
    }
}