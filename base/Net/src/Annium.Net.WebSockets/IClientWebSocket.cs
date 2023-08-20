using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IClientWebSocket : ISendingReceivingWebSocket
{
    ValueTask ConnectAsync(Uri uri, CancellationToken ct);
    ValueTask DisconnectAsync();
    ValueTask<WebSocketReceiveStatus> ListenAsync(CancellationToken ct);
}