using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IReceivingWebSocket
{
    /// <summary>
    /// Event is invoked, when text message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> TextReceived;

    /// <summary>
    /// Event is invoked, when binary message arrives
    /// </summary>
    event Action<ReadOnlyMemory<byte>> BinaryReceived;

    /// <summary>
    /// Start listening until websocket is closed
    /// </summary>
    /// <param name="ct">cancellation token</param>
    /// <returns>socket close status (canceled and aborted are treated as normal)</returns>
    Task<WebSocketCloseStatus> ListenAsync(CancellationToken ct);
}