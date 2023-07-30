using System.Threading.Tasks;

namespace Annium.Net.WebSockets.Benchmark;

public interface IServerWebSocket : ISendingReceivingWebSocket
{
    Task DisconnectAsync();
}