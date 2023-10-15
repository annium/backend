using System;
using System.Threading.Tasks;
using Annium.Mesh.Client;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Tests.System.Client.Clients;

public class TestServerClient : IAsyncDisposable
{
    public DemoClient Demo { get; }
    public event Action OnConnected = delegate { };
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly IClient _client;

    public TestServerClient(IClient client)
    {
        Demo = new DemoClient(client);
        _client = client;
        _client.OnDisconnected += status => OnDisconnected(status);
        _client.OnError += exception => OnError(exception);
    }

    public Task ConnectAsync() =>
        _client.ConnectAsync();

    public void Disconnect() =>
        _client.Disconnect();

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}