using System;
using Annium.Core.Internal;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

internal static class ProcessorExtensions
{
    public static TModel InitModel<TModel>(this IProcessor processor, ContextualType type, Func<Namespace, string, TModel> factory)
        where TModel : ModelBase
    {
        var name = type.FriendlyName();
        if (type.Type.IsGenericType)
            name = name[..name.IndexOf('<')];

        var model = factory(type.GetNamespace(), name);
        processor.Trace($"Initialized {type.FriendlyName()} model as {model}");

        return model;
    }

    public static void ProcessGenericArguments(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        var typeGenericArguments = type.GetGenericArguments();
        foreach (var argumentType in typeGenericArguments)
        {
            processor.Trace($"Process {type.FriendlyName()} generic argument {argumentType.FriendlyName()}");
            ctx.Process(argumentType);
        }
    }

    public static void ProcessBaseType(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        if (type.BaseType is null)
            return;

        processor.Trace($"Process {type.FriendlyName()} base type {type.BaseType.FriendlyName()}");
        ctx.Process(type.BaseType);
    }

    public static void ProcessInterfaces(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        foreach (var @interface in type.GetInterfaces())
        {
            processor.Trace($"Process {type.FriendlyName()} interface {@interface.FriendlyName()}");
            ctx.Process(@interface);
        }
    }

    public static void ProcessMembers(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        foreach (var member in type.GetMembers())
        {
            processor.Trace($"Process {type.FriendlyName()} member {member.AccessorType.FriendlyName()} {member.Name}");
            ctx.Process(member.AccessorType);
        }
    }
}