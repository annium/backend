using System.Threading.Tasks;

namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Factory for creating client connections
/// </summary>
public interface IClientConnectionFactory
{
    /// <summary>
    /// Creates a new client connection instance
    /// </summary>
    /// <returns>A new client connection</returns>
    IClientConnection Create();
}

/// <summary>
/// Factory for creating client connections with a specific context type
/// </summary>
/// <typeparam name="TContext">The type of context used for connection creation</typeparam>
public interface IClientConnectionFactory<TContext>
{
    /// <summary>
    /// Creates a new managed connection asynchronously using the provided context
    /// </summary>
    /// <param name="context">The context containing connection parameters</param>
    /// <returns>A task that represents the asynchronous operation and returns a managed connection</returns>
    Task<IManagedConnection> CreateAsync(TContext context);
}
