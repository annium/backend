using System;

namespace Annium.Net.WebSockets.Obsolete.Internal;

[Obsolete]
internal static class Constants
{
    /// <summary>
    /// Ping frame
    /// </summary>
    public static readonly ReadOnlyMemory<byte> PingFrame = new(new byte[] { 0x09 });

    /// <summary>
    /// Pong frame
    /// </summary>
    public static readonly ReadOnlyMemory<byte> PongFrame = new(new byte[] { 0x0A });
}