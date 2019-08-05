using System;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing.TestAdapter
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<TestConverter>(new TestConverter(Constants.ExecutorUri));
            services.AddSingleton<TestResultConverter>();
        }
    }
}