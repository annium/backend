using System;
using System.Threading.Tasks;
using Annium.Mesh.Client;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Tests.System.Client.Clients;

public class TestServerManagedClient : IAsyncDisposable
{
    public DemoClient Demo { get; }
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };
    public event Action<Exception> OnError = delegate { };
    private readonly IManagedClient _client;

    public TestServerManagedClient(IManagedClient client)
    {
        Demo = new DemoClient(client);
        _client = client;
        _client.OnDisconnected += status => OnDisconnected(status);
        _client.OnError += exception => OnError(exception);
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}