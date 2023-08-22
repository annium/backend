using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IServerManagedWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketCloseResult> IsClosed { get; }
    Task DisconnectAsync();
}