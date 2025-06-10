using System;
using System.Threading.Tasks;
using Annium.Net.Http;

namespace Annium.AspNetCore.IntegrationTesting;

/// <summary>
/// Factory for creating web application instances for integration testing
/// </summary>
public interface IWebApplicationFactory : IAsyncDisposable
{
    /// <summary>
    /// Resolves a service of the specified type from the application's service provider
    /// </summary>
    /// <typeparam name="T">The type of service to resolve</typeparam>
    /// <returns>The resolved service instance</returns>
    T Resolve<T>()
        where T : notnull;

    /// <summary>
    /// Resolves a service of the specified type from the application's service provider
    /// </summary>
    /// <param name="type">The type of service to resolve</param>
    /// <returns>The resolved service instance</returns>
    object Resolve(Type type);

    /// <summary>
    /// Gets an HTTP request instance configured for the test application
    /// </summary>
    /// <returns>An HTTP request instance</returns>
    IHttpRequest GetHttpRequest();

    /// <summary>
    /// Creates a WebSocket client connected to the specified endpoint
    /// </summary>
    /// <typeparam name="TWebSocketClient">The type of WebSocket client to create</typeparam>
    /// <param name="endpoint">The WebSocket endpoint to connect to</param>
    /// <returns>A task that represents the asynchronous operation containing the WebSocket client</returns>
    Task<TWebSocketClient> GetWebSocketClientAsync<TWebSocketClient>(string endpoint)
        where TWebSocketClient : class, IAsyncDisposable;
}
