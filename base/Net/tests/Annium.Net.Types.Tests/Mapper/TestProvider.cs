using System.Collections.Generic;
using Annium.Core.DependencyInjection;
using Annium.Net.Types.Models;
using Annium.Net.Types.Refs;
using Annium.Net.Types.Tests.Base.Mapper;
using Microsoft.Extensions.DependencyInjection;
using Namotion.Reflection;

namespace Annium.Net.Types.Tests.Mapper;

internal class TestProvider : ITestProvider
{
    public IReadOnlyCollection<IModel> Models => _mapper.GetModels();
    private IModelMapper _mapper = default!;

    public void ConfigureContainer(ServiceContainer container)
    {
        container.AddRuntime(GetType().Assembly);
    }

    public void Setup(ServiceProvider sp)
    {
        _mapper = sp.Resolve<IModelMapper>();
    }

    public IRef Map(ContextualType type) => _mapper.Map(type);
}