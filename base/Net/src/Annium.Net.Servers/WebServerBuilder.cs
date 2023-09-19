using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.Servers.Internal;

namespace Annium.Net.Servers;

public static class WebServerBuilder
{
    public static IWebServerBuilder New(IServiceProvider sp, Uri uri)
    {
        if (uri.Scheme != "http")
            throw new InvalidOperationException("Only http uri is supported by web server");

        return new WebServerBuilderInstance(sp, uri);
    }
}

public interface IWebServerBuilder
{
    IWebServerBuilder WithHttp(Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> handler);
    IWebServerBuilder WithWebSockets(Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handler);
    IWebServer Build();
}

file class WebServerBuilderInstance : IWebServerBuilder
{
    private readonly IServiceProvider _sp;
    private readonly Uri _uri;
    private Func<IServiceProvider, HttpListenerContext, CancellationToken, Task>? _handleHttp;
    private Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task>? _handleWebSocket;

    public WebServerBuilderInstance(IServiceProvider sp, Uri uri)
    {
        _sp = sp;
        _uri = uri;
    }

    public IWebServerBuilder WithHttp(Func<IServiceProvider, HttpListenerContext, CancellationToken, Task> handler)
    {
        _handleHttp = handler;

        return this;
    }

    public IWebServerBuilder WithWebSockets(Func<IServiceProvider, HttpListenerWebSocketContext, CancellationToken, Task> handler)
    {
        _handleWebSocket = handler;

        return this;
    }

    public IWebServer Build()
    {
        return new WebServer(_sp, _uri, _handleHttp, _handleWebSocket);
    }
}