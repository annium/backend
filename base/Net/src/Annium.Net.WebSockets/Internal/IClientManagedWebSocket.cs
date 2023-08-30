using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal;

internal interface IClientManagedWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketCloseResult> IsClosed { get; }
    Task<bool> ConnectAsync(Uri uri, CancellationToken ct);
    Task DisconnectAsync();
}