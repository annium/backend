using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging;
using Annium.Logging.Shared;
using Annium.Logging.Xunit;
using Annium.Net.Http;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Annium.AspNetCore.IntegrationTesting.Internal;

/// <summary>
/// Wrapper for web application factory that implements IWebApplicationFactory interface
/// </summary>
/// <typeparam name="TEntryPoint">The entry point class for the application</typeparam>
internal class WrappedWebApplicationFactory<TEntryPoint> : IWebApplicationFactory
    where TEntryPoint : class
{
    /// <summary>
    /// The underlying web application factory
    /// </summary>
    private readonly WebApplicationFactory<TEntryPoint> _appFactory;

    /// <summary>
    /// Output helper
    /// </summary>
    private readonly ITestOutputHelper _outputHelper;

    /// <summary>
    /// The factory for creating HTTP requests
    /// </summary>
    private readonly Lazy<IHttpRequestFactory> _httpRequestFactory;

    /// <summary>
    /// The disposable box for managing resources that need cleanup
    /// </summary>
    private AsyncDisposableBox _disposable = Disposable.AsyncBox(VoidLogger.Instance);

    /// <summary>
    /// Initializes a new instance of the WrappedWebApplicationFactory class
    /// </summary>
    /// <param name="appFactory">The underlying web application factory</param>
    /// <param name="outputHelper">Output helper</param>
    public WrappedWebApplicationFactory(WebApplicationFactory<TEntryPoint> appFactory, ITestOutputHelper outputHelper)
    {
        _appFactory = appFactory;
        _outputHelper = outputHelper;
        _httpRequestFactory = new Lazy<IHttpRequestFactory>(InitHttpRequestFactory, true);
        _disposable += _appFactory as IAsyncDisposable;
    }

    /// <summary>
    /// Resolves a service of the specified type from the application's service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <returns>The resolved service instance</returns>
    public T Resolve<T>()
        where T : notnull => _appFactory.Services.Resolve<T>();

    /// <summary>
    /// Resolves a service of the specified type from the application's service provider
    /// </summary>
    /// <param name="type">The type of service to resolve</param>
    /// <returns>The resolved service instance</returns>
    public object Resolve(Type type) => _appFactory.Services.Resolve(type);

    /// <summary>
    /// Gets an HTTP request instance configured for the test application
    /// </summary>
    /// <returns>An HTTP request instance</returns>
    public IHttpRequest GetHttpRequest()
    {
        return _httpRequestFactory.Value.New();
    }

    /// <summary>
    /// Creates a WebSocket client connected to the specified endpoint
    /// </summary>
    /// <typeparam name="TWebSocketClient">The type of WebSocket client to create</typeparam>
    /// <param name="endpoint">The WebSocket endpoint to connect to</param>
    /// <returns>A task that represents the asynchronous operation containing the WebSocket client</returns>
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

    /// <summary>
    /// Initializes and configures the HTTP client for testing
    /// </summary>
    /// <returns>The configured HTTP client</returns>
    private IHttpRequestFactory InitHttpRequestFactory()
    {
        var httpClient = _appFactory.CreateClient();

        var services = new ServiceContainer();
        services.AddRuntime(GetType().Assembly);
        services.AddTime().WithRelativeTime().SetDefault();
        services.AddLogging();
        services.Add(_outputHelper).AsSelf().Singleton();
        services.AddHttpRequestFactory(_ => httpClient, true);
        services.AddSerializers().WithJson();

        var sp = services.BuildServiceProvider();
        _disposable += sp as IAsyncDisposable;
        sp.UseLogging(x => x.UseTestOutput());

        var httpRequestFactory = sp.Resolve<IHttpRequestFactory>();

        return httpRequestFactory;
    }

    /// <summary>
    /// Disposes the wrapped web application factory and cleans up resources
    /// </summary>
    /// <returns>A value task that represents the asynchronous dispose operation</returns>
    public ValueTask DisposeAsync()
    {
        return _disposable.DisposeAsync();
    }
}
