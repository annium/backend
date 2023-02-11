using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Mapper;

public abstract class TestBase
{
    protected IReadOnlyCollection<IModel> Models => _mapper.GetModels();
    private readonly IModelMapper _mapper;

    protected TestBase()
    {
        _mapper = new ServiceContainer()
            .AddModelMapper()
            .BuildServiceProvider()
            .Resolve<IModelMapper>();
    }

    protected IRef Map(ContextualType type) => _mapper.Map(type);
}