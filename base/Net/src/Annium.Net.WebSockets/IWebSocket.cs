using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets
{
    public interface ISendingWebSocket
    {
        Task SendAsync<T>(T data, CancellationToken token);
        Task SendAsync(string data, CancellationToken token);
        Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken token);
    }

    public interface IReceivingWebSocket
    {
        MessageFormat Format { get; }
        Task<(bool isClosed, T data)> ReceiveAsync<T>(CancellationToken token);
        Task<(bool isClosed, string data)> ReceiveTextAsync(CancellationToken token);
        Task<(bool isClosed, byte[] data)> ReceiveBinaryAsync(CancellationToken token);
    }
}