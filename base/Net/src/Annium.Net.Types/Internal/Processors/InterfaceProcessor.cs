using System.Collections.Generic;
using System.Linq;
using Annium.Core.Internal;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal.Processors;

internal class InterfaceProcessor : IProcessor
{
    public bool Process(ContextualType type, Nullability nullability, IProcessingContext ctx)
    {
        if (!type.Type.IsInterface)
            return false;

        var pure = type.Type.GetPure().ToContextualType();
        if (ctx.IsRegistered(pure.Type))
            return true;

        var model = this.InitModel(pure, static (ns, name) => new InterfaceModel(ns, name));
        ctx.Register(pure.Type, model);

        ProcessType(type, ctx);
        CompleteModel(pure, model, ctx);

        return true;
    }

    private void ProcessType(ContextualType type, IProcessingContext ctx)
    {
        var typeGenericArguments = type.GetGenericArguments();
        foreach (var argumentType in typeGenericArguments)
        {
            this.Trace($"Process {type.FriendlyName()} generic argument {argumentType.FriendlyName()}");
            ctx.Process(argumentType);
        }

        foreach (var @interface in type.GetInterfaces())
        {
            this.Trace($"Process {type.FriendlyName()} interface {@interface.FriendlyName()}");
            ctx.Process(@interface);
        }

        foreach (var member in type.GetMembers())
        {
            this.Trace($"Process {type.FriendlyName()} member {member.AccessorType.FriendlyName()} {member.Name}");
            ctx.Process(member.AccessorType);
        }
    }

    private void CompleteModel(ContextualType type, InterfaceModel model, IProcessingContext ctx)
    {
        var typeGenericArguments = type.GetGenericArguments();
        var genericArguments = new List<IRef>(typeGenericArguments.Length);
        foreach (var genericArgument in typeGenericArguments)
        {
            this.Trace($"Resolve {type.FriendlyName()} generic argument {genericArgument.FriendlyName()} ref");
            genericArguments.Add(ctx.GetRef(genericArgument));
        }

        model.SetArgs(genericArguments);

        var interfaces = new List<IRef>(type.GetInterfaces().Count);
        foreach (var @interface in type.GetInterfaces())
        {
            if (MapperConfig.IsIgnored(@interface))
            {
                this.Trace($"Resolve ignore {type.FriendlyName()} interface {@interface.FriendlyName()} ref");
                continue;
            }

            this.Trace($"Resolve {type.FriendlyName()} interface {@interface.FriendlyName()} ref");
            interfaces.Add(ctx.GetRef(@interface));
        }

        model.SetInterfaces(interfaces);

        var fields = type.GetMembers()
            .Select(member =>
            {
                this.Trace($"Resolve {type.FriendlyName()} member {member.AccessorType.FriendlyName()} {member.Name} ref");
                return new FieldModel(ctx.GetRef(member.AccessorType), member.Name);
            })
            .ToArray();
        model.SetFields(fields);
    }
}