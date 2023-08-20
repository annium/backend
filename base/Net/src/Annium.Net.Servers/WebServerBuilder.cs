using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers.Internal;

namespace Annium.Net.Servers;

public static class WebServerBuilder
{
    public static IWebServerBuilder New(Uri uri)
    {
        if (uri.Scheme != "http")
            throw new InvalidOperationException("Only http uri is supported by web server");

        return new WebServerBuilderInstance(uri);
    }
}

public interface IWebServerBuilder
{
    IWebServerBuilder WithHttp(Func<HttpListenerContext, CancellationToken, Task> handler);
    IWebServerBuilder WithWebSockets(Func<HttpListenerWebSocketContext, CancellationToken, Task> handler);
    IWebServer Build();
}

file class WebServerBuilderInstance : IWebServerBuilder
{
    private readonly Uri _uri;
    private Func<HttpListenerContext, CancellationToken, Task>? _handleHttp;
    private Func<HttpListenerWebSocketContext, CancellationToken, Task>? _handleWebSocket;

    public WebServerBuilderInstance(Uri uri)
    {
        _uri = uri;
    }

    public IWebServerBuilder WithHttp(Func<HttpListenerContext, CancellationToken, Task> handler)
    {
        _handleHttp = handler;

        return this;
    }

    public IWebServerBuilder WithWebSockets(Func<HttpListenerWebSocketContext, CancellationToken, Task> handler)
    {
        _handleWebSocket = handler;

        return this;
    }

    public IWebServer Build()
    {
        return new WebServer(_uri, _handleHttp, _handleWebSocket);
    }
}