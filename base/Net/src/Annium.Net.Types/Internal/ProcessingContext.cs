using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Internal.Processors;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal sealed record ProcessingContext : IProcessingContext
{
    private readonly ConcurrentDictionary<Type, ITypeModel> _models = new();

    public void Process(ContextualType type) => Processor.Process(type, this);

    public void Process(ContextualType type, Nullability nullability) => Processor.Process(type, nullability, this);

    public ModelRef GetRef(ContextualType type)
    {
        throw new NotImplementedException();
    }

    public void Register(Type typeType, TypeModelBase model)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<ITypeModel> GetModels() => _models.Values.ToArray();
}