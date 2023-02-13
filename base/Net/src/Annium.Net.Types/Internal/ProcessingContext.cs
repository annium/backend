using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Internal.Referrers;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal sealed record ProcessingContext : IMapperProcessingContext
{
    private readonly Dictionary<Type, ModelBase> _models = new();
    private readonly Processor _processor;
    private readonly Referrer _referrer;

    public ProcessingContext(Processor processor, Referrer referrer)
    {
        _processor = processor;
        _referrer = referrer;
    }

    public void Process(ContextualType type) => _processor.Process(type, this);
    public void Process(ContextualType type, Nullability nullability) => _processor.Process(type, nullability, this);
    public IRef GetRef(ContextualType type) => _referrer.GetRef(type, this);
    public IRef GetRef(ContextualType type, Nullability nullability) => _referrer.GetRef(type, nullability, this);

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