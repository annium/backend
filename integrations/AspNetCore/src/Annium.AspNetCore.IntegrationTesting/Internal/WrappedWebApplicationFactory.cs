using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging;
using Annium.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Annium.AspNetCore.IntegrationTesting.Internal;

internal class WrappedWebApplicationFactory<TEntryPoint> : IWebApplicationFactory
    where TEntryPoint : class
{
    private readonly WebApplicationFactory<TEntryPoint> _appFactory;
    private readonly Lazy<HttpClient> _httpClient;
    private readonly IHttpRequestFactory _httpRequestFactory;
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    public WrappedWebApplicationFactory(
        WebApplicationFactory<TEntryPoint> appFactory
    )
    {
        _appFactory = appFactory;
        _httpClient = new Lazy<HttpClient>(InitHttpClient, true);
        _httpRequestFactory = Resolve<IHttpRequestFactory>();
        _disposable += _appFactory as IAsyncDisposable;
    }

    public T Resolve<T>()
        where T : notnull
        => _appFactory.Services.Resolve<T>();

    public object Resolve(Type type)
        => _appFactory.Services.Resolve(type);

    public IHttpRequest GetHttpRequest()
    {
        return _httpRequestFactory.New().UseClient(_httpClient.Value);
    }

    public async Task<TWebSocketClient> GetWebSocketClientAsync<TWebSocketClient>(string endpoint)
        where TWebSocketClient : class, IAsyncDisposable
    {
        var wsUri = new UriBuilder(_appFactory.Server.BaseAddress) { Scheme = "ws", Path = endpoint }.Uri;
        var wsClient = _appFactory.Server.CreateWebSocketClient();
        var ws = await wsClient.ConnectAsync(wsUri, CancellationToken.None);

        // delay before returning socket to let server init ReceiveAsync
        await Task.Delay(50);

        var clientFactory = Resolve<Func<WebSocket, Task<TWebSocketClient>>>();
        var client = await clientFactory(ws);
        _disposable += client;

        return client;
    }

    private HttpClient InitHttpClient()
    {
        var httpClient = _appFactory.CreateClient();
        _disposable += httpClient;

        return httpClient;
    }

    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}