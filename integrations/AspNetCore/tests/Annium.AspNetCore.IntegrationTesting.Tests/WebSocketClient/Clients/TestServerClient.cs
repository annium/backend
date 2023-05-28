using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Client;

namespace Annium.AspNetCore.IntegrationTesting.Tests.WebSocketClient.Clients;

public class TestServerClient : IAsyncDisposable
{
    public DemoClient Demo { get; }
    public bool IsConnected => _client.IsConnected;
    public event Func<Task> ConnectionLost = () => Task.CompletedTask;
    public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
    private readonly IClient _client;

    public TestServerClient(IClient client)
    {
        Demo = new DemoClient(client);
        _client = client;
        _client.ConnectionLost += () => ConnectionLost.Invoke();
        _client.ConnectionRestored += () => ConnectionRestored.Invoke();
    }

    public Task ConnectAsync(CancellationToken ct = default) =>
        _client.ConnectAsync(ct);

    public Task DisconnectAsync() =>
        _client.DisconnectAsync();

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}