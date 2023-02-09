using System.Linq;
using Annium.Core.Primitives;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Models;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class StructProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        Process(type, ctx);

        var pure = type.Type.GetPure();
        var model = Build(pure.ToContextualType(), ctx);

        ctx.Register(pure, model);

        return true;
    }

    private static void Process(ContextualType type, IProcessingContext ctx)
    {
        foreach (var argumentType in type.GenericArguments)
            ctx.Process(argumentType);

        if (type.BaseType is not null && !MapperConfig.IsIgnored(type.BaseType))
            ctx.Process(type.BaseType);

        var interfaces = type.GetInterfaces()
            .Where(x => !MapperConfig.IsIgnored(x))
            .ToArray();
        foreach (var @interface in interfaces)
            ctx.Process(@interface);

        foreach (var member in type.GetMembers())
            ctx.Process(member.AccessorType);
    }

    private static StructModel Build(ContextualType type, IProcessingContext ctx)
    {
        var name = type.Type.FriendlyName();
        if (type.Type.IsGenericType)
            name = name[..name.IndexOf('<')];

        var builder = StructModelBuilder.Init(type.GetNamespace(), name);

        var genericArguments = type.GenericArguments.Select(ctx.GetRef).ToArray();
        builder.GenericArguments(genericArguments);

        if (type.BaseType is not null && !MapperConfig.IsIgnored(type.BaseType))
            builder.Base(ctx.GetRef(type.BaseType));

        var interfaces = type.GetInterfaces()
            .Where(x => !MapperConfig.IsIgnored(x))
            .Select(ctx.GetRef)
            .ToArray();
        builder.Interfaces(interfaces);

        var fields = type.GetMembers()
            .Select(x => new FieldModel(ctx.GetRef(x.AccessorType), x.Name))
            .ToArray();
        builder.Fields(fields);

        return builder.Build();
    }
}