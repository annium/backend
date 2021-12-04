using System;
using Annium.Core.DependencyInjection;

namespace Annium.Testing.TestAdapter;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.Add(new TestConverter(Constants.ExecutorUri)).Singleton();
        container.Add<TestResultConverter>().Singleton();
    }
}