using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Internal;

public interface IServerManagedWebSocket : ISendingReceivingWebSocket
{
    Task<WebSocketCloseResult> IsClosed { get; }
    Task DisconnectAsync();
}