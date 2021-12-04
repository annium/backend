using System;
using System.Reactive;
using System.Threading;
using Annium.Core.Primitives;
using Annium.Net.WebSockets;
using Annium.Serialization.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Serialization;

internal static class SendingWebSocketExtensions
{
    public static IObservable<Unit> SendWith<T>(
        this ISendingWebSocket socket,
        T value,
        Serializer serializer,
        CancellationToken ct
    )
    {
        switch (serializer.Instance)
        {
            case ISerializer<ReadOnlyMemory<byte>> x:
                return socket.Send(x.Serialize(value), ct);
            case ISerializer<string> x:
                return socket.Send(x.Serialize(value), ct);
            default:
                throw new NotImplementedException(
                    $"Sending with {serializer.Instance.GetType().FriendlyName()} serializer is not implemented"
                );
        }
    }

    public static IObservable<Unit> SendSerialized(
        this ISendingWebSocket socket,
        object value,
        CancellationToken ct
    )
    {
        switch (value)
        {
            case ReadOnlyMemory<byte> x:
                return socket.Send(x, ct);
            case string x:
                return socket.Send(x, ct);
            default:
                throw new NotImplementedException(
                    $"Sending of {value.GetType().FriendlyName()} is not implemented"
                );
        }
    }
}