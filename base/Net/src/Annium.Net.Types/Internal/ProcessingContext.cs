using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Internal.Referrers;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal sealed record ProcessingContext : IProcessingContext
{
    private readonly Dictionary<Type, ModelBase> _models = new();

    public void Process(ContextualType type) => Processor.Process(type, this);
    public void Process(ContextualType type, Nullability nullability) => Processor.Process(type, nullability, this);
    public IRef GetRef(ContextualType type) => Referrer.GetRef(type, this);
    public IRef GetRef(ContextualType type, Nullability nullability) => Referrer.GetRef(type, nullability, this);

    public IRef RequireRef(ContextualType type)
    {
        var model = _models.GetValueOrDefault(type.Type) ?? throw new InvalidOperationException($"No model is registered for type {type.Type}");

        return model switch
        {
            EnumModel x      => new EnumRef(x.Namespace.ToString(), x.Name),
            InterfaceModel x => new InterfaceRef(x.Namespace.ToString(), x.Name, x.Args.ToArray()),
            StructModel x    => new StructRef(x.Namespace.ToString(), x.Name, x.Args.ToArray()),
            _                => throw new ArgumentOutOfRangeException($"Unexpected model {model}")
        };
    }

    public bool IsRegistered(Type type)
    {
        return _models.ContainsKey(type);
    }

    public void Register(Type type, ModelBase model)
    {
        _models.TryAdd(type, model);
    }

    public IReadOnlyCollection<ModelBase> GetModels() => _models.Values.ToArray();
}