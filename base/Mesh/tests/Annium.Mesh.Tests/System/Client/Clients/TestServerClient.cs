using System;
using System.Threading.Tasks;
using Annium.Mesh.Client;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Tests.System.Client.Clients;

/// <summary>
/// Test client wrapper for mesh server interactions, providing demo client functionality and connection lifecycle management.
/// </summary>
public class TestServerClient : IAsyncDisposable
{
    /// <summary>
    /// Gets the demo client for interacting with demo mesh server functionality.
    /// </summary>
    public DemoClient Demo { get; }

    /// <summary>
    /// Event raised when the client successfully connects to the server.
    /// </summary>
    public event Action OnConnected = delegate { };

    /// <summary>
    /// Event raised when the client disconnects from the server.
    /// </summary>
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when an error occurs in the client.
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// The underlying mesh client for server communication.
    /// </summary>
    private readonly IClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestServerClient"/> class with the specified mesh client.
    /// </summary>
    /// <param name="client">The underlying mesh client instance.</param>
    public TestServerClient(IClient client)
    {
        Demo = new DemoClient(client);
        _client = client;
        _client.OnConnected += () => OnConnected();
        _client.OnDisconnected += status => OnDisconnected(status);
        _client.OnError += exception => OnError(exception);
    }

    /// <summary>
    /// Connects to the mesh server asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous connect operation.</returns>
    public Task ConnectAsync() => _client.ConnectAsync();

    /// <summary>
    /// Disconnects from the mesh server.
    /// </summary>
    public void Disconnect() => _client.Disconnect();

    /// <summary>
    /// Disposes the client and releases all resources asynchronously.
    /// </summary>
    /// <returns>A value task representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync() => _client.DisposeAsync();
}
