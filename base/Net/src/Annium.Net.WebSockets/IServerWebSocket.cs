using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IServerWebSocket : ISendingReceivingWebSocket
{
    ValueTask DisconnectAsync();
    ValueTask<WebSocketReceiveStatus> ListenAsync(CancellationToken ct);
}