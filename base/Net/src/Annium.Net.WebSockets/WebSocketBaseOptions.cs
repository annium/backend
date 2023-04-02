using System;
using Annium.Net.WebSockets.Internal;
using NodaTime;

namespace Annium.Net.WebSockets;

public record WebSocketBaseOptions
{
    public ActiveKeepAlive? ActiveKeepAlive { get; set; }
    public PassiveKeepAlive? PassiveKeepAlive { get; set; }
}

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

public record PassiveKeepAlive
{
    public static PassiveKeepAlive Create() => new();

    public ReadOnlyMemory<byte> PingFrame => Constants.PingFrame;
    public ReadOnlyMemory<byte> PongFrame => Constants.PongFrame;
}