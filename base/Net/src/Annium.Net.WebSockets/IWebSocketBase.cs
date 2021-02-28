using System;
using System.Net.WebSockets;
using System.Reactive;
using System.Threading;

namespace Annium.Net.WebSockets
{
    public interface ISendingReceivingWebSocket : ISendingWebSocket, IReceivingWebSocket
    {
    }

    public interface ISendingWebSocket : IWebSocketBase, IDisposable
    {
        IObservable<Unit> Send(string data, CancellationToken token);
        IObservable<Unit> Send(ReadOnlyMemory<byte> data, CancellationToken token);
    }

    public interface IReceivingWebSocket : IWebSocketBase, IDisposable
    {
        IObservable<SocketMessage> Listen();
        IObservable<string> ListenText();
        IObservable<ReadOnlyMemory<byte>> ListenBinary();
    }

    public interface IWebSocketBase
    {
        WebSocketState State { get; }
    }
}