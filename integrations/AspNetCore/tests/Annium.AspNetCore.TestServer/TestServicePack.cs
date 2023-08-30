using System;
using Annium.Core.DependencyInjection;

namespace Annium.AspNetCore.TestServer;

public class TestServicePack : ServicePackBase
{
    public TestServicePack()
    {
        Add<BaseServicePack>();
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddTime().WithRelativeTime().SetDefault();
    }
}