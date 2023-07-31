using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Annium.Debug;

namespace Annium.Net.WebSockets.Obsolete.Internal;

[Obsolete]
internal static class Configurator
{
    public static Configuration GetConfiguration(
        IObservable<SocketMessage> observable,
        Encoding encoding,
        Func<ReadOnlyMemory<byte>, IObservable<Unit>> send,
        WebSocketBaseOptions options
    )
    {
        IKeepAliveMonitor keepAliveMonitor = new KeepAliveMonitorStub();
        var keepAliveFrames = new List<ReadOnlyMemory<byte>>();
        var disposable = Disposable.Box();

        // if active - send pings/count pongs via monitor
        if (options.ActiveKeepAlive is not null)
        {
            var opts = options.ActiveKeepAlive;
            keepAliveFrames.Add(opts.PongFrame);
            keepAliveMonitor = new KeepAliveMonitor(observable, send, opts);
        }

        // if passive - listen pings, respond with pongs
        if (options.PassiveKeepAlive is not null)
        {
            var opts = options.PassiveKeepAlive;
            keepAliveFrames.Add(opts.PingFrame);
            var pingPong = new PingPong();
            disposable += observable
                .Where(x =>
                    x.Type == WebSocketMessageType.Binary &&
                    x.Data.Length == 1 &&
                    x.Data.Span.SequenceEqual(opts.PingFrame.Span)
                )
                .DoParallelAsync(async _ =>
                {
                    pingPong.Trace("KeepAlive: ping -> pong");
                    await send(opts.PongFrame);
                })
                .Subscribe();
        }

        // configure messageObservable
        var messageObservable = observable;
        if (options.ActiveKeepAlive is not null || options.PassiveKeepAlive is not null)
            messageObservable = messageObservable
                .Where(x =>
                    // not binary
                    x.Type != WebSocketMessageType.Binary ||
                    // or binary, but not single-byte frame
                    x.Data.Length != 1 ||
                    // or single-byte binary frame, but not keepAlive frame
                    keepAliveFrames.All(f => !x.Data.Span.SequenceEqual(f.Span))
                );

        // configure binaryObservable
        var binaryObservable = observable
            .Where(x =>
                // binary
                x.Type == WebSocketMessageType.Binary &&
                (
                    // not single-byte frame
                    x.Data.Length != 1 ||
                    // or single-byte binary frame, but not keepAlive frame
                    keepAliveFrames.All(f => !x.Data.Span.SequenceEqual(f.Span))
                )
            )
            .Select(x => x.Data);

        // configure textObservable (just for symmetry)
        var textObservable = observable
            .Where(x => x.Type == WebSocketMessageType.Text)
            .Select(x => encoding.GetString(x.Data.Span));

        return new Configuration(
            keepAliveMonitor,
            messageObservable,
            binaryObservable,
            textObservable,
            disposable
        );
    }
}

internal sealed record Configuration(
    IKeepAliveMonitor KeepAliveMonitor,
    IObservable<SocketMessage> MessageObservable,
    IObservable<ReadOnlyMemory<byte>> BinaryObservable,
    IObservable<string> TextObservable,
    IDisposable Disposable
);

file sealed record PingPong;