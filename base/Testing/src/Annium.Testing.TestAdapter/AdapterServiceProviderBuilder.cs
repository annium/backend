using System;
using Annium.Core.DependencyInjection;
using Annium.Core.DependencyInjection.Obsolete;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Annium.Testing.TestAdapter
{
    internal static class AdapterServiceProviderBuilder
    {
        public static IServiceProvider Build(IDiscoveryContext discoveryContext)
        {
            var services = new ServiceCollection();
            services.AddSingleton(TestingConfigurationReader.Read(discoveryContext));

            var factory = new ServiceProviderFactory();
            return factory.CreateServiceProvider(factory.CreateBuilder(services).UseServicePack<Testing.ServicePack>());
        }
    }
}