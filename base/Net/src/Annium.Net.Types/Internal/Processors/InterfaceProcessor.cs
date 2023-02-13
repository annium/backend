using Annium.Core.Internal;
using Annium.Net.Types.Internal.Extensions;
using Annium.Net.Types.Internal.Helpers;
using Annium.Net.Types.Models;
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
        {
            this.Trace($"Process {type.FriendlyName()} - skip, already registered");
            return true;
        }

        var model = this.InitModel(pure, static (ns, name) => new InterfaceModel(ns, name));
        ctx.Register(pure.Type, model);

        ProcessType(type, ctx);
        CompleteModel(pure, model, ctx);

        return true;
    }

    private void ProcessType(ContextualType type, IProcessingContext ctx)
    {
        this.ProcessGenericArguments(type, ctx);
        this.ProcessInterfaces(type, ctx);
        this.ProcessMembers(type, ctx);
    }

    private void CompleteModel(ContextualType type, InterfaceModel model, IProcessingContext ctx)
    {
        model.SetArgs(this.ResolveGenericArguments(type, ctx));
        model.SetInterfaces(this.ResolveInterfaces(type, ctx));
        model.SetFields(this.ResolveFields(type, ctx));
    }
}