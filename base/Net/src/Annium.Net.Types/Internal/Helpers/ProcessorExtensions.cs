using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Internal;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Helpers;

internal static class ProcessorExtensions
{
    public static TModel InitModel<TModel>(this IProcessor processor, ContextualType type, Func<Namespace, bool, string, TModel> factory)
        where TModel : IModel
    {
        var name = type.PureName();
        var model = factory(type.GetNamespace(), type.Type.IsAbstract, name);
        processor.Trace($"Initialized {type.FriendlyName()} model as {model}");

        return model;
    }

    public static void ProcessImplementations(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Process {type.FriendlyName()} implementations");
        var implementations = ctx.GetImplementations(type);
        if (implementations.Count == 0)
        {
            processor.Trace($"Process {type.FriendlyName()} implementations - no implementations");
            return;
        }

        processor.Trace($"Process {type.FriendlyName()} {implementations.Count} implementation(s)");
        foreach (var implementation in implementations)
            ctx.Process(implementation);
    }

    public static void ProcessBaseType(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Process {type.FriendlyName()} base type");
        if (type.BaseType is null)
        {
            processor.Trace($"Process {type.FriendlyName()} base type - no base type");
            return;
        }

        processor.Trace($"Process {type.FriendlyName()} base type {type.BaseType.FriendlyName()}");
        ctx.Process(type.BaseType);
    }

    public static void ProcessInterfaces(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Process {type.FriendlyName()} interfaces");
        foreach (var @interface in type.GetOwnInterfaces())
        {
            processor.Trace($"Process {type.FriendlyName()} interface {@interface.FriendlyName()}");
            ctx.Process(@interface);
        }
    }

    public static void ProcessMembers(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Process {type.FriendlyName()} members");
        foreach (var member in type.GetOwnMembers())
        {
            processor.Trace($"Process {type.FriendlyName()} member {member.AccessorType.FriendlyName()} {member.Name}");
            ctx.Process(member.AccessorType);
        }
    }

    public static IReadOnlyList<GenericParameterRef> ResolveGenericArguments(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Resolve {type.FriendlyName()} generic argument refs");
        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = new List<GenericParameterRef>(typeGenericArguments.Length);
        foreach (var genericArgument in typeGenericArguments)
        {
            processor.Trace($"Resolve {type.FriendlyName()} generic argument {genericArgument.FriendlyName()} ref");
            genericArguments.Add((GenericParameterRef) ctx.GetRef(genericArgument));
        }

        return genericArguments;
    }

    public static StructRef? ResolveBaseType(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Resolve {type.FriendlyName()} base type ref");
        if (type.BaseType is null)
        {
            processor.Trace($"Resolve {type.FriendlyName()} base type ref - no base type");
            return null;
        }

        if (MapperConfig.IsIgnored(type.BaseType))
        {
            processor.Trace($"Resolve ignore {type.FriendlyName()} base type {type.BaseType.FriendlyName()} ref");
            return null;
        }

        processor.Trace($"Resolve {type.FriendlyName()} base type {type.BaseType.FriendlyName()} ref");

        return (StructRef) ctx.GetRef(type.BaseType);
    }

    public static IReadOnlyList<InterfaceRef> ResolveInterfaces(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Resolve {type.FriendlyName()} interface refs");
        var interfaces = type.GetOwnInterfaces();
        var interfaceRefs = new List<InterfaceRef>(interfaces.Count);
        foreach (var @interface in interfaces)
        {
            if (MapperConfig.IsIgnored(@interface))
            {
                processor.Trace($"Resolve ignore {type.FriendlyName()} interface {@interface.FriendlyName()} ref");
                continue;
            }

            processor.Trace($"Resolve {type.FriendlyName()} interface {@interface.FriendlyName()} ref");
            interfaceRefs.Add((InterfaceRef) ctx.GetRef(@interface));
        }

        return interfaceRefs;
    }

    public static IReadOnlyList<FieldModel> ResolveFields(this IProcessor processor, ContextualType type, IProcessingContext ctx)
    {
        processor.Trace($"Resolve {type.FriendlyName()} field models");
        var fields = type.GetOwnMembers()
            .Select(member =>
            {
                processor.Trace($"Resolve {type.FriendlyName()} member {member.AccessorType.FriendlyName()} {member.Name} ref");
                return new FieldModel(ctx.GetRef(member.AccessorType), member.Name);
            })
            .ToArray();

        return fields;
    }
}