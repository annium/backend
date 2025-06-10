using System.Threading.Tasks;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Transport.InMemory.Internal;

/// <summary>
/// Factory for creating client connections in the in-memory transport implementation
/// </summary>
internal class ClientConnectionFactory : IClientConnectionFactory, IClientConnectionFactory<None>
{
    /// <summary>
    /// The connection hub used to create and manage connections
    /// </summary>
    private readonly IConnectionHub _hub;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConnectionFactory"/> class
    /// </summary>
    /// <param name="hub">The connection hub used to create connections</param>
    public ClientConnectionFactory(IConnectionHub hub)
    {
        _hub = hub;
    }

    /// <summary>
    /// Creates a new client connection
    /// </summary>
    /// <returns>A new client connection instance</returns>
    public IClientConnection Create()
    {
        var connection = _hub.Create();

        return connection;
    }

    /// <summary>
    /// Creates and connects a new managed client connection asynchronously
    /// </summary>
    /// <param name="context">The connection context (not used in in-memory implementation)</param>
    /// <returns>A task that represents the asynchronous operation, containing the managed connection</returns>
    public Task<IManagedConnection> CreateAsync(None context)
    {
        var connection = _hub.Create();

        connection.Connect();

        return Task.FromResult<IManagedConnection>(connection);
    }
}
