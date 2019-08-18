using System;
using Annium.Core.DependencyInjection;
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

            return new ServiceProviderBuilder(services)
                .UseServicePack<Testing.ServicePack>()
                .Build();
        }
    }
}