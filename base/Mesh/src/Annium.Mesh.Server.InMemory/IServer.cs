using System.Threading;
using System.Threading.Tasks;

namespace Annium.Mesh.Server.InMemory;

/// <summary>
/// Represents an in-memory mesh server
/// </summary>
public interface IServer
{
    /// <summary>
    /// Runs the server asynchronously until cancellation is requested
    /// </summary>
    /// <param name="ct">Cancellation token to stop the server</param>
    /// <returns>A task that represents the asynchronous server operation</returns>
    Task RunAsync(CancellationToken ct = default);
}
