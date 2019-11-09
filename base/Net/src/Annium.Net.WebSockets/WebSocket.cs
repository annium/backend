using System;
using System.Buffers;
using System.IO;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Internal;
using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Net.WebSockets
{
    public abstract class WebSocket<TNativeSocket> : ISendingWebSocket, IReceivingWebSocket, IDisposable
    where TNativeSocket : NativeWebSocket
    {
        private const int bufferSize = 65536;

        public MessageFormat Format { get; }
        protected readonly TNativeSocket socket;
        private readonly UTF8Encoding encoding = new UTF8Encoding();

        public WebSocket(
            TNativeSocket socket,
            MessageFormat format
        )
        {
            this.socket = socket;
            Format = format;
        }

        public IObservable<int> Send<T>(T data, CancellationToken token) =>
            Send(data.Serialize(Format), token);

        public IObservable<int> Send(string data, CancellationToken token) =>
            Send(encoding.GetBytes(data).AsMemory(), WebSocketMessageType.Text, token);

        public IObservable<int> Send(ReadOnlyMemory<byte> data, CancellationToken token) =>
             Send(data, WebSocketMessageType.Binary, token);

        public IObservable<T> Listen<T>() where T : notnull =>
            Listen(WebSocketMessageType.Binary, x => x.Deserialize<T>(Format));

        public IObservable<string> ListenText() =>
            Listen(WebSocketMessageType.Binary, x => encoding.GetString(x.Span));

        public IObservable<ReadOnlyMemory<byte>> ListenBinary() =>
            Listen(WebSocketMessageType.Binary, x => x);

        private IObservable<int> Send(
            ReadOnlyMemory<byte> data,
            WebSocketMessageType messageType,
            CancellationToken token
        )
        {
            return Observable.FromAsync(async () =>
            {
                // TODO: implement chunking, if needed
                await socket.SendAsync(
                    buffer: data,
                    messageType: messageType,
                    endOfMessage: true,
                    cancellationToken: token
                );

                return data.Length;
            });
        }

        private IObservable<T> Listen<T>(
            WebSocketMessageType? type,
            Func<ReadOnlyMemory<byte>, T> convert
        ) => Observable.Create<T>(async (observer, token) =>
        {
            var pool = ArrayPool<byte>.Shared;
            var buffer = pool.Rent(bufferSize);

            while (await ReceiveAsync(observer, buffer, type, convert, token)) { }

            pool.Return(buffer);

            return () => socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        });

        private async ValueTask<bool> ReceiveAsync<T>(
            IObserver<T> observer,
            Memory<byte> buffer,
            WebSocketMessageType? type,
            Func<ReadOnlyMemory<byte>, T> convert,
            CancellationToken token
        )
        {
            try
            {
                using var stream = new MemoryStream();
                ValueWebSocketReceiveResult result;
                do
                {
                    result = await socket.ReceiveAsync(buffer, token);

                    // if closing - send close and return
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _ = socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);

                        observer.OnCompleted();

                        return false;
                    }

                    // skip unexpected message type
                    if (result.MessageType == type)
                        continue;

                    stream.Write(buffer.Slice(0, result.Count).Span);
                }
                while (!result.EndOfMessage);

                observer.OnNext(convert(stream.ToArray()));

                return true;
            }
            //  remote party closed connection w/o handshake
            catch (WebSocketException)
            {
                observer.OnCompleted();

                return false;
            }
            // token was canceled
            catch (OperationCanceledException)
            {
                observer.OnCompleted();

                return false;
            }
            catch (Exception e)
            {
                observer.OnError(e);

                return false;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing)
                socket.Dispose();

            disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}