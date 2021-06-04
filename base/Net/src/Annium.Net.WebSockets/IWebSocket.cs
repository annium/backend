using System.Threading;
using System.Threading.Tasks;

namespace Annium.Net.WebSockets
{
    public interface IWebSocket : ISendingReceivingWebSocket
    {
        Task DisconnectAsync();
    }
}