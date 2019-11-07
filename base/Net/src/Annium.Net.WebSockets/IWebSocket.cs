using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;

namespace Annium.Net.WebSockets
{
    public interface ISendingWebSocket : IDisposable
    {
        Task<IBooleanResult> SendAsync<T>(T data, CancellationToken token);
        Task<IBooleanResult> SendAsync(string data, CancellationToken token);
        Task<IBooleanResult> SendAsync(ReadOnlyMemory<byte> data, CancellationToken token);
    }

    public interface IReceivingWebSocket : IDisposable
    {
        MessageFormat Format { get; }
        Task<IBooleanResult<SocketResponse<T>>> ReceiveAsync<T>(CancellationToken token);
        Task<IBooleanResult<SocketResponse<string>>> ReceiveTextAsync(CancellationToken token);
        Task<IBooleanResult<SocketResponse<byte[]>>> ReceiveBinaryAsync(CancellationToken token);
    }
}