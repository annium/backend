using System;
using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest
    {
        private readonly ConcurrentDictionary<Type, IRequest> requestSamples = new ConcurrentDictionary<Type, IRequest>();

        protected IRequest GetRequest<TStartup, TServicePack>()
        where TStartup : class
        where TServicePack : ServicePackBase, new()
        {
            return GetRequest<TStartup, TServicePack>(r => r);
        }

        protected IRequest GetRequest<TStartup, TServicePack>(
            Func<IRequest, IRequest> configureRequest
        )
        where TStartup : class
        where TServicePack : ServicePackBase, new()
        {
            return configureRequest(GetRequestBase<TStartup>(hostBuilder =>
            {
                var serviceProviderFactory = new ServiceProviderFactory(providerBuilder => providerBuilder.UseServicePack<TServicePack>());
                hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
            }));
        }

        protected IRequest GetRequest<TStartup>(
            Action<IHostBuilder> configureHost
        )
        where TStartup : class
        {
            return GetRequest<TStartup>(configureHost, r => r);
        }

        protected IRequest GetRequest<TStartup>(
            Action<IHostBuilder> configureHost,
            Func<IRequest, IRequest> configureRequest
        )
        where TStartup : class
        {
            return configureRequest(GetRequestBase<TStartup>(configureHost));
        }

        private IRequest GetRequestBase<TStartup>(Action<IHostBuilder> configureHost) where TStartup : class =>
            requestSamples.GetOrAdd(typeof(TStartup), _ =>
            {
                var client = new TestWebApplicationFactory<TStartup>(configureHost).CreateClient();

                return Http.Open().UseClient(client);
            }).Clone();
    }
}