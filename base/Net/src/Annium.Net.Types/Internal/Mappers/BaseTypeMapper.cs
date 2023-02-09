using Annium.Core.Internal;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class BaseTypeMapper
{
    public static ITypeModel? Map(ContextualType type)
    {
        var model = BaseType.GetFor(type.Type);
        if (model is null)
            return null;

        Log.Trace($"Mapped {type} -> {model}");

        return model;
    }
}