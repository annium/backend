using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Primitives;
using Annium.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Annium.AspNetCore.IntegrationTesting.Internal
{
    internal class WrappedWebApplicationFactory<TStartup> : IWebApplicationFactory
        where TStartup : class
    {
        private readonly WebApplicationFactory<TStartup> _appFactory;
        private readonly Lazy<IHttpRequest> _httpRequest;
        private AsyncDisposableBox _disposable = Disposable.AsyncBox();

        public WrappedWebApplicationFactory(
            WebApplicationFactory<TStartup> appFactory
        )
        {
            _appFactory = appFactory;
            _httpRequest = new Lazy<IHttpRequest>(InitHttpRequest, true);
            _disposable += _appFactory as IAsyncDisposable;
        }

        public T Resolve<T>()
            where T : notnull
            => _appFactory.Services.Resolve<T>();

        public object Resolve(Type type)
            => _appFactory.Services.Resolve(type);

        public IHttpRequest GetHttpRequest() => _httpRequest.Value.Clone();

        public async Task<TWebSocketClient> GetWebSocketClientAsync<TWebSocketClient>(string endpoint)
            where TWebSocketClient : class, IAsyncDisposable
        {
            var wsUri = new UriBuilder(_appFactory.Server.BaseAddress) { Scheme = "ws", Path = endpoint }.Uri;
            var wsClient = _appFactory.Server.CreateWebSocketClient();
            var ws = await wsClient.ConnectAsync(wsUri, CancellationToken.None);

            var clientFactory = Resolve<Func<WebSocket, TWebSocketClient>>();
            var client = clientFactory(ws);
            _disposable += client;

            return client;
        }

        private IHttpRequest InitHttpRequest()
        {
            var httpClient = _appFactory.CreateClient();
            _disposable += httpClient;
            var httpRequestFactory = Resolve<IHttpRequestFactory>();

            var request = httpRequestFactory.New().UseClient(httpClient);

            return request;
        }

        public ValueTask DisposeAsync()
        {
            return _disposable.DisposeAsync();
        }
    }
}