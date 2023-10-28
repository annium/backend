using System;
using System.Threading.Tasks;
using Annium.Net.Http;

namespace Annium.AspNetCore.IntegrationTesting;

public interface IWebApplicationFactory : IAsyncDisposable
{
    T Resolve<T>()
        where T : notnull;

    object Resolve(Type type);

    IHttpRequest GetHttpRequest();

    Task<TWebSocketClient> GetWebSocketClientAsync<TWebSocketClient>(string endpoint)
        where TWebSocketClient : class, IAsyncDisposable;
}
