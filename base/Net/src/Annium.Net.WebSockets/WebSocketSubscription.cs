using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Data.Operations;
using Annium.Net.WebSockets.Internal;
using BaseResponse = Annium.Data.Operations.IBooleanResult<Annium.Net.WebSockets.SocketResponse<string>>;

namespace Annium.Net.WebSockets
{
    public class WebSocketSubscription : IWebSocketSubscription
    {
        private readonly IReceivingWebSocket socket;
        private readonly IDictionary<Action<BaseResponse>, Func<string, bool>> handlers =
            new Dictionary<Action<BaseResponse>, Func<string, bool>>();

        public WebSocketSubscription(IReceivingWebSocket socket)
        {
            this.socket = socket;
        }

        public Action Subscribe(Action<BaseResponse> handler) =>
            Subscribe(handler, _ => true);

        public Action Subscribe(Action<BaseResponse> handler, Func<string, bool> filter)
        {
            lock (handlers)
                handlers[handler] = filter;

            return () => handlers.Remove(handler);
        }

        public Action Subscribe<T>(Action<IBooleanResult<SocketResponse<T>>> handler) =>
            Subscribe(handler, _ => true);

        public Action Subscribe<T>(Action<IBooleanResult<SocketResponse<T>>> handler, Func<string, bool> filter)
        {
            void innerHandler(BaseResponse x)
            {
                var e = new SocketResponse<T>(x.Data.IsSocketOpen, x.Data.Data.Deserialize<T>(socket.Format));
                handler(x.IsSuccess ? Result.Success(e) : Result.Failure(e).Join(x));
            };

            lock (handlers)
                handlers[innerHandler] = filter;

            return () => handlers.Remove(innerHandler);
        }

        public async Task ListenAsync(CancellationToken token)
        {
            var result = Result.Success(new SocketResponse<string>(true, string.Empty));

            while (!token.IsCancellationRequested && result.Data.IsSocketOpen && !disposedValue)
            {
                result = await socket.ReceiveTextAsync(token);

                foreach (var (handler, filter) in handlers.ToList())
                    if (filter(result.Data.Data)) handler(result);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
            {
                handlers.Clear();
            }

            disposedValue = true;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}