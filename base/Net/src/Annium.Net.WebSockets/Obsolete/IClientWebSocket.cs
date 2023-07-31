using System;
using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Obsolete;

[Obsolete]
public interface IClientWebSocket : ISendingReceivingWebSocket
{
    Task ConnectAsync(Uri uri, CancellationToken ct);
    Task DisconnectAsync();
}