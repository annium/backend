using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IServerWebSocket : ISendingReceivingWebSocket
{
    Task DisconnectAsync();
    Task<WebSocketReceiveStatus> ListenAsync(CancellationToken ct);
}