using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection.Container;

namespace Annium.Mesh.Tests;

/// <summary>
/// Defines the contract for test behaviors that configure and run mesh servers for testing purposes.
/// </summary>
public interface IBehavior
{
    /// <summary>
    /// Registers services required for the behavior in the dependency injection container.
    /// </summary>
    /// <param name="container">The service container to register services in.</param>
    static abstract void Register(IServiceContainer container);

    /// <summary>
    /// Runs the mesh server asynchronously for the duration of the test.
    /// </summary>
    /// <param name="ct">The cancellation token to stop the server.</param>
    /// <returns>A task representing the asynchronous server operation.</returns>
    Task RunServerAsync(CancellationToken ct);
}
