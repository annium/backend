using System;
using System.Net.WebSockets;
using System.Reactive;
using System.Threading;

namespace Annium.Net.WebSockets;

public interface ISendingReceivingWebSocket : ISendingWebSocket, IReceivingWebSocket
{
}

public interface ISendingWebSocket : IWebSocketBase, IAsyncDisposable
{
    IObservable<Unit> Send(string data, CancellationToken ct = default);
    IObservable<Unit> Send(ReadOnlyMemory<byte> data, CancellationToken ct = default);
}

public interface IReceivingWebSocket : IWebSocketBase, IAsyncDisposable
{
    IObservable<SocketMessage> Listen();
    IObservable<string> ListenText();
    IObservable<ReadOnlyMemory<byte>> ListenBinary();
}

public interface IWebSocketBase
{
    WebSocketState State { get; }
}