using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Internal.Referrers;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal sealed record ProcessingContext : IProcessingContext
{
    private readonly ConcurrentDictionary<Type, TypeModelBase> _models = new();

    public void Process(ContextualType type) => Processor.Process(type, this);
    public void Process(ContextualType type, Nullability nullability) => Processor.Process(type, nullability, this);
    public ModelRef GetRef(ContextualType type) => Referrer.GetRef(type, this);
    public ModelRef GetRef(ContextualType type, Nullability nullability) => Referrer.GetRef(type, nullability, this);

    public ModelRef RequireRef(ContextualType type)
    {
        var model = _models.GetValueOrDefault(type.Type) ?? throw new InvalidOperationException($"No model is registered for type {type.Type}");

        return new ModelRef(model.Namespace.ToString(), model.Name);
    }

    public void Register(Type type, TypeModelBase model)
    {
        _models.TryAdd(type, model);
    }

    public IReadOnlyCollection<TypeModelBase> GetModels() => _models.Values.ToArray();
}