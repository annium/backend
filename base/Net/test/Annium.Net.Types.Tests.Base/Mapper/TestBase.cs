using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Base.Mapper;

public abstract class TestBase
{
    private readonly ITestProvider _testProvider;
    protected IReadOnlyCollection<IModel> Models => _testProvider.Models;

    protected TestBase(ITestProvider testProvider)
    {
        _testProvider = testProvider;
        var container = new ServiceContainer();
        container.AddModelMapper();
        testProvider.ConfigureContainer(container);
        var sp = container.BuildServiceProvider();
        testProvider.Setup(sp);
    }

    protected IRef Map(ContextualType type) => _testProvider.Map(type);
}