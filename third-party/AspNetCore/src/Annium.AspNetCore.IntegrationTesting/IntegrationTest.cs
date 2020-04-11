using System;
using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest
    {
        private readonly ConcurrentDictionary<Type, IHttpRequest> requestSamples = new ConcurrentDictionary<Type, IHttpRequest>();

        protected IHttpRequest GetRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder
        )
        where TStartup : class
        {
            return GetRequestBase<TStartup>(hostBuilder =>
            {
                var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
                hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
            });
        }

        protected IHttpRequest GetRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder,
            Action<IServiceCollection> configureServices
        )
        where TStartup : class
        {
            return GetRequestBase<TStartup>(hostBuilder =>
            {
                var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
                hostBuilder.ConfigureServices((ctx, services) => configureServices(services));
                hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
            });
        }

        protected IHttpRequest GetRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder,
            Func<IHttpRequest, IHttpRequest> configureRequest
        )
        where TStartup : class
        {
            return configureRequest(GetRequest<TStartup>(configureBuilder));
        }

        protected IHttpRequest GetRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder,
            Action<IServiceCollection> configureServices,
            Func<IHttpRequest, IHttpRequest> configureRequest
        )
        where TStartup : class
        {
            return configureRequest(GetRequest<TStartup>(configureBuilder, configureServices));
        }

        private IHttpRequest GetRequestBase<TStartup>(Action<IHostBuilder> configureHost) where TStartup : class =>
            requestSamples.GetOrAdd(typeof(TStartup), _ =>
            {
                var client = new TestWebApplicationFactory<TStartup>(configureHost).CreateClient();

                return Http.Open().UseClient(client);
            }).Clone();
    }
}