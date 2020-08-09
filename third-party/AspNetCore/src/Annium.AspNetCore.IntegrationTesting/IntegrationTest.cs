using System;
using System.Collections.Concurrent;
using Annium.Core.DependencyInjection;
using Annium.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest : IDisposable
    {
        private readonly ConcurrentBag<IDisposable> _disposables = new ConcurrentBag<IDisposable>();

        private readonly ConcurrentDictionary<Type, IHttpRequest> _requestSamples =
            new ConcurrentDictionary<Type, IHttpRequest>();

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
            _requestSamples.GetOrAdd(typeof(TStartup), _ =>
            {
                var appFactory = new TestWebApplicationFactory<TStartup>(configureHost);
                _disposables.Add(appFactory);
                var client = appFactory.CreateClient();
                var requestFactory = appFactory.Services.GetRequiredService<IHttpRequestFactory>();

                return requestFactory.New().UseClient(client);
            }).Clone();

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }
}