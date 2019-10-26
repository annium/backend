using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Internal;

namespace Annium.Net.WebSockets
{
    public class WebSocketSubscription : IWebSocketSubscription
    {
        private readonly IReceivingWebSocket socket;
        private IDictionary<Action<string>, Func<string, bool>> handlers =
            new Dictionary<Action<string>, Func<string, bool>>();

        public WebSocketSubscription(IReceivingWebSocket socket)
        {
            this.socket = socket;
        }

        public Action Subscribe(Action<string> handler) => Subscribe(handler, _ => true);

        public Action Subscribe(Action<string> handler, Func<string, bool> filter)
        {
            lock (handlers)
                handlers[handler] = filter;

            return () => handlers.Remove(handler);
        }

        public Action Subscribe<T>(Action<T> handler) => Subscribe(handler, _ => true);

        public Action Subscribe<T>(Action<T> handler, Func<string, bool> filter)
        {
            Action<string> innerHandler = (string data) => handler(data.Deserialize<T>(socket.Format));
            lock (handlers)
                handlers[innerHandler] = filter;

            return () => handlers.Remove(innerHandler);
        }

        public async Task ListenAsync(CancellationToken token)
        {
            var isClosed = false;
            string raw;

            while (!token.IsCancellationRequested && !isClosed && !disposedValue)
            {
                (isClosed, raw) = await socket.ReceiveTextAsync(token);

                foreach (var (handler, filter) in handlers)
                    if (filter(raw)) handler(raw);
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