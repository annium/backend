using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
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

        private DisposableBox _disposable = Disposable.Box();
        private readonly ConcurrentDictionary<Type, object> _appFactoryCache = new();
        private readonly ConcurrentDictionary<Type, IHttpRequest> _httpRequestCache = new();
        private readonly ConcurrentDictionary<Type, object> _webSocketClientCache = new();

        #region HttpRequest

        protected IHttpRequest GetHttpRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder
        )
            where TStartup : class
            => GetHttpRequestBase<TStartup>(ConfigureHost(configureBuilder));

        protected IHttpRequest GetHttpRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder,
            Action<IServiceContainer> configureServices
        )
            where TStartup : class
            => GetHttpRequestBase<TStartup>(ConfigureHost(configureBuilder, configureServices));

        protected IHttpRequest GetHttpRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder,
            Func<IHttpRequest, IHttpRequest> configureRequest
        )
            where TStartup : class
            => configureRequest(GetHttpRequest<TStartup>(configureBuilder));

        protected IHttpRequest GetHttpRequest<TStartup>(
            Action<IServiceProviderBuilder> configureBuilder,
            Action<IServiceContainer> configureServices,
            Func<IHttpRequest, IHttpRequest> configureRequest
        )
            where TStartup : class
            => configureRequest(GetHttpRequest<TStartup>(configureBuilder, configureServices));

        #endregion

        #region SocketClient

        protected TWebSocketClient GetWebSocketClient<TStartup, TWebSocketClient>(
            Action<IServiceProviderBuilder> configureBuilder,
            string endpoint = ""
        )
            where TStartup : class
            where TWebSocketClient : class
            => GetWebSocketClientBase<TStartup, TWebSocketClient>(ConfigureHost(configureBuilder), endpoint);

        protected TWebSocketClient GetWebSocketClient<TStartup, TWebSocketClient>(
            Action<IServiceProviderBuilder> configureBuilder,
            Action<IServiceContainer> configureServices,
            string endpoint = ""
        )
            where TStartup : class
            where TWebSocketClient : class
            => GetWebSocketClientBase<TStartup, TWebSocketClient>(ConfigureHost(configureBuilder, configureServices), endpoint);

        #endregion

        #region internal

        private IHttpRequest GetHttpRequestBase<TStartup>(Action<IHostBuilder> configureHost) where TStartup : class =>
            _httpRequestCache.GetOrAdd(typeof(TStartup), (_, configure) =>
            {
                var appFactory = GetAppFactory<TStartup>(configure);

                var httpClient = appFactory.CreateClient();
                var httpRequestFactory = appFactory.Services.Resolve<IHttpRequestFactory>();

                var request = httpRequestFactory.New().UseClient(httpClient);

                return request;
            }, configureHost).Clone();

        private TWebSocketClient GetWebSocketClientBase<TStartup, TWebSocketClient>(
            Action<IHostBuilder> configureHost,
            string endpoint
        )
            where TStartup : class
            where TWebSocketClient : class
            => (TWebSocketClient) _webSocketClientCache.GetOrAdd(typeof(TStartup), (_, configure) =>
            {
                var appFactory = GetAppFactory<TStartup>(configure);

                var wsUri = new UriBuilder(appFactory.Server.BaseAddress) { Scheme = "ws", Path = endpoint }.Uri;
                var ws = appFactory.Server.CreateWebSocketClient().ConnectAsync(wsUri, CancellationToken.None).Await();

                var client = appFactory.Services.Resolve<Func<WebSocket, TWebSocketClient>>()(ws)!;

                return client;
            }, configureHost);

        private WebApplicationFactory<TStartup> GetAppFactory<TStartup>(Action<IHostBuilder> configureHost) where TStartup : class =>
            (WebApplicationFactory<TStartup>) _appFactoryCache.GetOrAdd(typeof(TStartup), (_, configure) =>
            {
                var appFactory = new TestWebApplicationFactory<TStartup>(configure);
                _disposable += appFactory;

                return appFactory;
            }, configureHost);

        #endregion

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}