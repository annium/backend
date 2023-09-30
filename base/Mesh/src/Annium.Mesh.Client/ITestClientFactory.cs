using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Infrastructure.WebSockets.Client;

public interface ITestClientFactory
{
    ITestClient Create(NativeWebSocket socket, ITestClientConfiguration configuration);
}