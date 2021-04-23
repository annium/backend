using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Annium.Core.Internal;
using Annium.Core.Primitives;

namespace Annium.Net.WebSockets.Internal
{
    internal static class Configurator
    {
        public static Configuration GetConfiguration(
            IObservable<SocketMessage> observable,
            Encoding encoding,
            Func<string, IObservable<Unit>> send,
            WebSocketBaseOptions options
        )
        {
            Func<ISendingReceivingWebSocket, Action, IAsyncDisposable> keepAliveMonitorFactory =
                (_, _) => Disposable.Create(() => Task.CompletedTask);
            var keepAliveMessages = new List<string>();
            var disposable = Disposable.AsyncBox();

            // if active - send pings/count pongs via monitor
            if (options.ActiveKeepAlive is not null)
            {
                var opts = options.ActiveKeepAlive;
                keepAliveMonitorFactory =
                    (socket, signal) => new KeepAliveMonitor(socket, encoding, opts, signal);
                keepAliveMessages.Add(opts.PongFrame);
            }

            // if passive - listen pings, respond with pongs
            if (options.PassiveKeepAlive is not null)
            {
                var opts = options.PassiveKeepAlive;
                disposable += observable
                    .Where(x => x.Type == WebSocketMessageType.Text)
                    .Select(x => encoding.GetString(x.Data.Span))
                    .Where(x => x == opts.PingFrame)
                    .SubscribeAsync(async _ =>
                    {
                        observable.Trace(() => $"KeepAlive: {opts.PingFrame} -> {opts.PongFrame}");
                        await send(opts.PongFrame);
                    });
                keepAliveMessages.Add(opts.PingFrame);
            }

            // configure binaryObservable (just for symmetry)
            var binaryObservable = observable
                .Where(x => x.Type == WebSocketMessageType.Binary)
                .Select(x => x.Data);

            // configure textObservable
            var textObservable = observable
                .Where(x => x.Type == WebSocketMessageType.Text)
                .Select(x => encoding.GetString(x.Data.Span));
            if (keepAliveMessages.Count > 0)
                textObservable = textObservable.Where(x => !keepAliveMessages.Contains(x));

            return new Configuration(
                binaryObservable,
                textObservable,
                keepAliveMonitorFactory,
                disposable
            );
        }
    }

    internal record Configuration(
        IObservable<ReadOnlyMemory<byte>> BinaryObservable,
        IObservable<string> TextObservable,
        Func<ISendingReceivingWebSocket, Action, IAsyncDisposable> KeepAliveMonitorFactory,
        IAsyncDisposable Disposable
    );
}