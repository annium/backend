using System;
using Annium.Net.WebSockets.Obsolete.Internal;
using NodaTime;

namespace Annium.Net.WebSockets.Obsolete;

[Obsolete]
public record WebSocketBaseOptions
{
    public ActiveKeepAlive? ActiveKeepAlive { get; set; }
    public PassiveKeepAlive? PassiveKeepAlive { get; set; }
}

[Obsolete]
public record ActiveKeepAlive : PassiveKeepAlive
{
    public static ActiveKeepAlive Create(
        uint pingInterval = 60,
        uint retries = 3
    ) => new()
    {
        PingInterval = Duration.FromSeconds(pingInterval),
        Retries = retries,
    };

    public Duration PingInterval { get; init; }
    public uint Retries { get; init; }
}

[Obsolete]
public record PassiveKeepAlive
{
    public static PassiveKeepAlive Create() => new();

    public ReadOnlyMemory<byte> PingFrame => Constants.PingFrame;
    public ReadOnlyMemory<byte> PongFrame => Constants.PongFrame;
}