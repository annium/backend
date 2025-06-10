using System.Threading.Tasks;

namespace Annium.Mesh.Server.Handlers;

/// <summary>
/// Base class for handlers that need to perform operations during connection lifecycle events.
/// </summary>
public abstract class LifeCycleHandlerBase
{
    /// <summary>
    /// Called when a connection starts. Override to perform custom startup logic.
    /// </summary>
    /// <returns>A task representing the asynchronous startup operation.</returns>
    public virtual Task StartAsync() => Task.CompletedTask;

    /// <summary>
    /// Called when a connection ends. Override to perform custom cleanup logic.
    /// </summary>
    /// <returns>A task representing the asynchronous cleanup operation.</returns>
    public virtual Task EndAsync() => Task.CompletedTask;
}
