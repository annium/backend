using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IServerManagedWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketReceiveStatus> IsClosed { get; }
    Task DisconnectAsync();
}