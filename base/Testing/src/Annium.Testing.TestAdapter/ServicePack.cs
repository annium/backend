using System;
using Annium.Core.DependencyInjection;
using Annium.Core.DependencyInjection.Obsolete;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing.TestAdapter
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton(new TestConverter(Constants.ExecutorUri));
            services.AddSingleton<TestResultConverter>();
        }
    }
}