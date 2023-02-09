using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Annium.Net.Types.Models;
using Namotion.Reflection;

namespace Annium.Net.Types.Internal;

internal sealed record MapperContext : IMapperContext
{
    private readonly ConcurrentDictionary<Type, ITypeModel> _models = new();
    private readonly IModelMapper _mapper;

    public MapperContext(IModelMapper mapper)
    {
        _mapper = mapper;
    }

    public void Init(ContextualType type)
    {
        throw new NotImplementedException();
    }

    public bool IsProcessing(ContextualType type)
    {
        throw new NotImplementedException();
    }

    public ITypeModel Map(ContextualType type, Nullability nullability) => throw new NotImplementedException();
    public ITypeModel Map(ContextualType type) => throw new NotImplementedException();

    public void Register(ContextualType type, ITypeModel model)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<ITypeModel> GetModels() => _models.Values.ToArray();
}

internal interface IMapperContext
{
    bool IsProcessing(ContextualType type);
    ITypeModel Map(ContextualType type, Nullability nullability);
    ITypeModel Map(ContextualType type);
}