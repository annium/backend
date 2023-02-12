using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Serialization.Abstractions;
using Namotion.Reflection;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public abstract class TestBase
{
    protected IReadOnlyCollection<IModel> Models
    {
        get
        {
            var raw = _serializer.Serialize(_mapper.GetModels());
            var models = _serializer.Deserialize<IReadOnlyCollection<ModelBase>>(raw);
            return models;
        }
    }

    private readonly IModelMapper _mapper;
    private readonly ISerializer<string> _serializer;

    protected TestBase()
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddModelMapper();
        container.AddSerializers()
            .WithJson(opts => { opts.ConfigureForNetTypes(); }, isDefault: true);
        var sp = container.BuildServiceProvider();
        _mapper = sp.Resolve<IModelMapper>();
        _serializer = sp.Resolve<ISerializer<string>>();
    }

    protected IRef Map(ContextualType type)
    {
        var originalRef = _mapper.Map(type);
        var raw = _serializer.Serialize(originalRef);
        var modelRef = _serializer.Deserialize<IRef>(raw);

        return modelRef;
    }
}