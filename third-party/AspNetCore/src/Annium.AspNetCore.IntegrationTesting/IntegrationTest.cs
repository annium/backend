using System;
using System.Collections.Concurrent;
using Annium.AspNetCore.IntegrationTesting.Internal;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Microsoft.Extensions.Hosting;

namespace Annium.AspNetCore.IntegrationTesting
{
    public class IntegrationTest : IDisposable
    {
        #region host configuration

        private static Action<IHostBuilder> ConfigureHost(
            Action<IServiceProviderBuilder> configureBuilder
        ) => hostBuilder =>
        {
            var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
            hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
        };

        private static Action<IHostBuilder> ConfigureHost(
            Action<IServiceProviderBuilder> configureBuilder,
            Action<IServiceContainer> configureServices
        ) => hostBuilder =>
        {
            var serviceProviderFactory = new ServiceProviderFactory(configureBuilder);
            hostBuilder.ConfigureServices((_, services) => configureServices(new ServiceContainer(services)));
            hostBuilder.UseServiceProviderFactory(serviceProviderFactory);
        };

        #endregion

        #region state

        private DisposableBox _disposable = Disposable.Box();
        private readonly ConcurrentDictionary<Type, IWebApplicationFactory> _cache = new();

        #endregion

        #region WebAppFactory cache access

        public IWebApplicationFactory GetAppFactory<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder
        )
            where TStartup : class =>
            GetAppFactory<TStartup>(ConfigureHost(configureBuilder));

        public IWebApplicationFactory GetAppFactory<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder,
            Action<IServiceContainer> configureServices
        )
            where TStartup : class =>
            GetAppFactory<TStartup>(ConfigureHost(configureBuilder, configureServices));

        private IWebApplicationFactory GetAppFactory<TStartup>(
            Action<IHostBuilder> configureHost
        )
            where TStartup : class =>
            _cache.GetOrAdd(typeof(TStartup), (_, configure) =>
            {
                var appFactory = new TestWebApplicationFactory<TStartup>(configure);
                _disposable += appFactory;

                return new WebApplicationFactoryWrapper<TStartup>(appFactory);
            }, configureHost);

        #endregion

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}