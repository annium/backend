using System;
using System.Threading.Tasks;
using Annium.Mesh.Client;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Tests.System.Client.Clients;

/// <summary>
/// Test managed client wrapper for mesh server interactions, providing demo client functionality and automatic reconnection handling.
/// </summary>
public class TestServerManagedClient : IAsyncDisposable
{
    /// <summary>
    /// Gets the demo client for interacting with demo mesh server functionality.
    /// </summary>
    public DemoClient Demo { get; }

    /// <summary>
    /// Event raised when the client disconnects from the server.
    /// </summary>
    public event Action<ConnectionCloseStatus> OnDisconnected = delegate { };

    /// <summary>
    /// Event raised when an error occurs in the client.
    /// </summary>
    public event Action<Exception> OnError = delegate { };

    /// <summary>
    /// The underlying managed mesh client for server communication.
    /// </summary>
    private readonly IManagedClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestServerManagedClient"/> class with the specified managed mesh client.
    /// </summary>
    /// <param name="client">The underlying managed mesh client instance.</param>
    public TestServerManagedClient(IManagedClient client)
    {
        Demo = new DemoClient(client);
        _client = client;
        _client.OnDisconnected += status => OnDisconnected(status);
        _client.OnError += exception => OnError(exception);
    }

    /// <summary>
    /// Disposes the client and releases all resources asynchronously.
    /// </summary>
    /// <returns>A value task representing the asynchronous disposal operation.</returns>
    public ValueTask DisposeAsync() => _client.DisposeAsync();
}
