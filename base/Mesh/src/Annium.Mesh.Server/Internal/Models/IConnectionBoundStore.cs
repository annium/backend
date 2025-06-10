using System;
using System.Threading.Tasks;

namespace Annium.Mesh.Server.Internal.Models;

/// <summary>
/// Defines a contract for stores that maintain connection-bound resources and require cleanup when connections are closed.
/// </summary>
internal interface IConnectionBoundStore
{
    /// <summary>
    /// Cleans up resources associated with the specified connection.
    /// </summary>
    /// <param name="connectionId">The identifier of the connection to clean up.</param>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    Task CleanupAsync(Guid connectionId);
}
