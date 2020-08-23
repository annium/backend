using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Core.WebAssembly
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddRuntimeTools(GetType().Assembly);
        }

        public override void Setup(IServiceProvider provider)
        {
            var typeManager = provider.GetRequiredService<ITypeManager>();
            Console.WriteLine($"TypeManager has {typeManager.Types.Count} types");
        }
    }
}