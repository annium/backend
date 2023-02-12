using System.Collections.Generic;
using System.Linq;
using Annium.Core.Internal;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal static class StructProcessor
{
    public static bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        var pure = type.Type.GetPure().ToContextualType();
        if (ctx.IsRegistered(pure.Type))
            return true;

        var model = InitModel(pure);
        ctx.Register(pure.Type, model);

        ProcessType(type, ctx);
        CompleteModel(pure, model, ctx);

        return true;
    }

    private static StructModel InitModel(ContextualType type)
    {
        var name = type.FriendlyName();
        if (type.Type.IsGenericType)
            name = name[..name.IndexOf('<')];

        return new StructModel(type.GetNamespace(), name);
    }

    private static void ProcessType(ContextualType type, IProcessingContext ctx)
    {
        var typeGenericArguments = type.GetGenericArguments();
        foreach (var argumentType in typeGenericArguments)
        {
            Log.Trace($"Process {type.FriendlyName()} generic argument {argumentType.FriendlyName()}");
            ctx.Process(argumentType);
        }

        if (type.BaseType is not null)
        {
            if (MapperConfig.IsIgnored(type.BaseType))
                Log.Trace($"Process ignore {type.FriendlyName()} base type {type.BaseType.FriendlyName()}");
            else
            {
                Log.Trace($"Process {type.FriendlyName()} base type {type.BaseType.FriendlyName()}");
                ctx.Process(type.BaseType);
            }
        }

        foreach (var @interface in type.GetInterfaces())
        {
            if (MapperConfig.IsIgnored(@interface))
            {
                Log.Trace($"Process ignore {type.FriendlyName()} interface {@interface.FriendlyName()}");
                continue;
            }

            Log.Trace($"Process {type.FriendlyName()} interface {@interface.FriendlyName()}");
            ctx.Process(@interface);
        }

        foreach (var member in type.GetMembers())
        {
            Log.Trace($"Process {type.FriendlyName()} member {member.AccessorType.FriendlyName()} {member.Name}");
            ctx.Process(member.AccessorType);
        }
    }

    private static void CompleteModel(ContextualType type, StructModel model, IProcessingContext ctx)
    {
        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = new List<IRef>(typeGenericArguments.Length);
        foreach (var genericArgument in typeGenericArguments)
        {
            Log.Trace($"Resolve {type.FriendlyName()} generic argument {genericArgument.FriendlyName()} ref");
            genericArguments.Add(ctx.GetRef(genericArgument));
        }

        model.SetArgs(genericArguments);

        if (type.BaseType is not null)
        {
            if (MapperConfig.IsIgnored(type.BaseType))
                Log.Trace($"Resolve ignore {type.FriendlyName()} base type {type.BaseType.FriendlyName()} ref");
            else
            {
                Log.Trace($"Resolve {type.FriendlyName()} base type {type.BaseType.FriendlyName()} ref");
                model.SetBase(ctx.GetRef(type.BaseType));
            }
        }

        var interfaces = new List<IRef>(type.GetInterfaces().Count);
        foreach (var @interface in type.GetInterfaces())
        {
            if (MapperConfig.IsIgnored(@interface))
            {
                Log.Trace($"Resolve ignore {type.FriendlyName()} interface {@interface.FriendlyName()} ref");
                continue;
            }

            Log.Trace($"Resolve {type.FriendlyName()} interface {@interface.FriendlyName()} ref");
            interfaces.Add(ctx.GetRef(@interface));
        }

        model.SetInterfaces(interfaces);

        var fields = type.GetMembers()
            .Select(member =>
            {
                Log.Trace($"Resolve {type.FriendlyName()} member {member.AccessorType.FriendlyName()} {member.Name} ref");
                return new FieldModel(ctx.GetRef(member.AccessorType), member.Name);
            })
            .ToArray();
        model.SetFields(fields);
    }
}