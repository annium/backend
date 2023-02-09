using System.Linq;
using System.Reflection;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Internal.Models;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Mappers;

internal static class StructMapper
{
    public static ITypeModel Map(ContextualType type, IMapperContext ctx)
    {
        var name = type.Type.FriendlyName();
        if (type.Type.IsGenericType)
            name = name[..name.IndexOf('<')];
        var builder = StructModelBuilder.Init(type.GetNamespace(), name);

        var genericArguments = type.GenericArguments.Select(ctx.Map).ToArray();
        builder.GenericArguments(genericArguments);

        if (type.BaseType is not null && !MapperConfig.IsIgnored(type.BaseType))
            builder.Base((StructModel) ctx.Map(type.BaseType));

        var interfaces = type.Type.GetInterfaces()
            .Select(x => x.ToContextualType())
            .Where(x => !MapperConfig.IsIgnored(x))
            .Select(ctx.Map)
            .OfType<StructModel>()
            .ToArray();
        builder.Interfaces(interfaces);

        var properties = type.Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new FieldModel(ctx.Map(x.ToContextualProperty().PropertyType), x.Name))
            .ToArray();
        var fields = type.Type
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new FieldModel(ctx.Map(x.ToContextualField().FieldType), x.Name))
            .ToArray();
        builder.Fields(properties.Concat(fields).ToArray());

        var model = builder.Build();
        Log.Trace($"Mapped {type} -> {model}");

        return model;
    }
}