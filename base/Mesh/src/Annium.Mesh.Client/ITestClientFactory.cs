using NativeWebSocket = System.Net.WebSockets.WebSocket;

namespace Annium.Mesh.Client;

public interface ITestClientFactory
{
    ITestClient Create(NativeWebSocket socket, ITestClientConfiguration configuration);
}