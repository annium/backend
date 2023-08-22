using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IClientManagedWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketReceiveStatus> IsClosed { get; }
    Task ConnectAsync(Uri uri, CancellationToken ct);
    Task DisconnectAsync();
}