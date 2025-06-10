using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client;

/// <summary>
/// Factory interface for creating mesh client instances
/// </summary>
public interface IClientFactory
{
    /// <summary>
    /// Creates a new client instance with the specified configuration
    /// </summary>
    /// <param name="configuration">The client configuration</param>
    /// <returns>A new client instance</returns>
    IClient Create(IClientConfiguration configuration);

    /// <summary>
    /// Creates a new managed client instance with the specified connection and configuration
    /// </summary>
    /// <param name="connection">The managed connection to use</param>
    /// <param name="configuration">The client configuration</param>
    /// <returns>A new managed client instance</returns>
    IManagedClient Create(IManagedConnection connection, IClientConfiguration configuration);
}
