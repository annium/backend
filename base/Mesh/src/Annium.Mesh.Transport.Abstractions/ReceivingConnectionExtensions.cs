using System;
using System.Reactive.Linq;

namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Extension methods for receiving connections
/// </summary>
public static class ReceivingConnectionExtensions
{
    /// <summary>
    /// Creates an observable sequence from the OnReceived event of a receiving connection
    /// </summary>
    /// <param name="connection">The receiving connection to observe</param>
    /// <returns>An observable sequence of received byte data</returns>
    public static IObservable<ReadOnlyMemory<byte>> Observe(this IReceivingConnection connection) =>
        Observable.FromEvent<ReadOnlyMemory<byte>>(x => connection.OnReceived += x, x => connection.OnReceived -= x);
}
