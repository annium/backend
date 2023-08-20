using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IServerWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketReceiveStatus> IsClosed { get; }
    Task DisconnectAsync();
}