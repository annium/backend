using Annium.Core.Internal;
using Annium.Core.Reflection;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class RecordMapper
{
    public static ITypeModel? Map(ContextualType type, IMapperContext ctx)
    {
        if (!MapperConfig.IsRecord(type))
            return null;

        var args = type.Type.GetTargetImplementation(MapperConfig.BaseRecordType)!.ToContextualType().GenericArguments[0].GenericArguments;
        var keyTypeModel = ctx.Map(args[0]);
        var valueTypeModel = ctx.Map(args[1]);

        var model = new RecordModel(keyTypeModel, valueTypeModel);
        Log.Trace($"Mapped {type} -> {model}");

        return model;
    }
}