using System.Threading.Tasks;

namespace Annium.Mesh.Transport.Abstractions;

/// <summary>
/// Factory for creating server connections with a specific context type
/// </summary>
/// <typeparam name="TContext">The type of context used for connection creation</typeparam>
public interface IServerConnectionFactory<TContext>
{
    /// <summary>
    /// Creates a new server connection asynchronously using the provided context
    /// </summary>
    /// <param name="context">The context containing connection parameters</param>
    /// <returns>A task that represents the asynchronous operation and returns a server connection</returns>
    Task<IServerConnection> CreateAsync(TContext context);
}
