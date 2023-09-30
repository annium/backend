using System;
using System.Threading.Tasks;
using Annium.Mesh.Client;

namespace Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient.Clients;

public class TestServerTestClient : IAsyncDisposable
{
    public DemoClient Demo { get; }
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;
    private readonly ITestClient _client;

    public TestServerTestClient(ITestClient client)
    {
        Demo = new DemoClient(client);
        _client = client;
        _client.ConnectionLost += () => ConnectionLost.Invoke();
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}