using System.Threading.Tasks;

namespace Annium.Net.WebSockets;

public interface IServerWebSocket : ISendingReceivingWebSocket
{
    Task DisconnectAsync();
}