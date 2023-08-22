using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IServerManagedWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketCloseStatus> IsClosed { get; }
    Task DisconnectAsync();
}