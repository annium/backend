using System;
using System.Threading;

namespace Annium.Net.WebSockets
{
    public interface ISendingWebSocket : IDisposable
    {
        IObservable<int> Send<T>(T data, CancellationToken token);
        IObservable<int> Send(string data, CancellationToken token);
        IObservable<int> Send(ReadOnlyMemory<byte> data, CancellationToken token);
    }

    public interface IReceivingWebSocket : IDisposable
    {
        IObservable<T> Listen<T>() where T : notnull;
        IObservable<string> ListenText();
        IObservable<ReadOnlyMemory<byte>> ListenBinary();
    }
}