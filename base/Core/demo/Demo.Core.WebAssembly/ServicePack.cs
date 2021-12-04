using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;

namespace Demo.Core.WebAssembly;

public class ServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddRuntimeTools(GetType().Assembly, false);
    }

    public override void Setup(IServiceProvider provider)
    {
        var typeManager = provider.Resolve<ITypeManager>();
        Console.WriteLine($"TypeManager has {typeManager.Types.Count} types");
    }
}