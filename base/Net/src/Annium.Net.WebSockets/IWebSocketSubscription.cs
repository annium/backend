using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using BaseResponse = Annium.Data.Operations.IBooleanResult<Annium.Net.WebSockets.SocketResponse<string>>;

namespace Annium.Net.WebSockets
{
    public interface IWebSocketSubscription : IDisposable
    {
        Action Subscribe(Action<BaseResponse> handler);
        Action Subscribe(Action<BaseResponse> handler, Func<string, bool> filter);
        Action Subscribe<T>(Action<IBooleanResult<SocketResponse<T>>> handler);
        Action Subscribe<T>(Action<IBooleanResult<SocketResponse<T>>> handler, Func<string, bool> filter);
        Task ListenAsync(CancellationToken token);
    }
}