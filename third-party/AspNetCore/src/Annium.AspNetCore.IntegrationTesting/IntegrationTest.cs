using System;
using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Net.Http;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest : IDisposable
    {
        private DisposableBox _disposable = Disposable.Box();
        private readonly ConcurrentDictionary<Type, IHttpRequest> _requestSamples = new();

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
            Action<IServiceContainer> configureServices
        )
            where TStartup : class
        {
            return GetRequestBase<TStartup>(hostBuilder =>
            {
                var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
                hostBuilder.ConfigureServices((ctx, services) => configureServices(new ServiceContainer(services)));
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
            Action<IServiceContainer> configureServices,
            Func<IHttpRequest, IHttpRequest> configureRequest
        )
            where TStartup : class
        {
            return configureRequest(GetRequest<TStartup>(configureBuilder, configureServices));
        }

        private IHttpRequest GetRequestBase<TStartup>(Action<IHostBuilder> configureHost) where TStartup : class =>
            _requestSamples.GetOrAdd(typeof(TStartup), _ =>
            {
                var appFactory = new TestWebApplicationFactory<TStartup>(configureHost);
                _disposable += appFactory;
                var client = appFactory.CreateClient();
                var requestFactory = appFactory.Services.Resolve<IHttpRequestFactory>();

                return requestFactory.New().UseClient(client);
            }).Clone();

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}