using System;
using System.Reactive.Linq;

namespace Annium.Mesh.Transport.Abstractions;

public static class ReceivingConnectionExtensions
{
    public static IObservable<ReadOnlyMemory<byte>> Observe(this IReceivingConnection connection) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(
            x => connection.OnReceived += x,
            x => connection.OnReceived -= x
        );
}