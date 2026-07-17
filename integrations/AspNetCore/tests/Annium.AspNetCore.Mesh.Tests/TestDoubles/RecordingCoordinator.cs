using System.Threading;
using System.Threading.Tasks;
using Annium.Mesh.Server;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.AspNetCore.Mesh.Tests.TestDoubles;

/// <summary>
/// Test double for <see cref="ICoordinator" /> that records the connection it was invoked with, blocks
/// inside <see cref="HandleAsync" /> until the test calls <see cref="Release" /> (simulating an in-progress
/// connection being handled), and records whether <see cref="Dispose" /> was invoked — used to pin the
/// <c>applicationLifetime.ApplicationStopping.Register(_coordinator.Dispose)</c> hookup.
/// </summary>
internal sealed class RecordingCoordinator : ICoordinator
{
    /// <summary>
    /// Completes with the connection passed to <see cref="HandleAsync" />, once it has been called.
    /// </summary>
    public Task<IServerConnection> Handled => _handled.Task;

    /// <summary>
    /// Completes once <see cref="Dispose" /> has been invoked.
    /// </summary>
    public Task DisposedSignal => _disposed.Task;

    /// <summary>
    /// Gets a value indicating whether <see cref="Dispose" /> was invoked.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Signals the connection passed to <see cref="HandleAsync" />.
    /// </summary>
    private readonly TaskCompletionSource<IServerConnection> _handled = new(
        TaskCreationOptions.RunContinuationsAsynchronously
    );

    /// <summary>
    /// Signals that the test is done inspecting the handled connection, releasing <see cref="HandleAsync" />.
    /// </summary>
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Signals once <see cref="Dispose" /> has been invoked.
    /// </summary>
    private readonly TaskCompletionSource _disposed = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Records <paramref name="connection" /> on <see cref="Handled" />, then blocks until <see cref="Release" />
    /// is called, simulating an in-progress connection being handled.
    /// </summary>
    /// <param name="connection">The connection to handle.</param>
    /// <returns>A task that completes once <see cref="Release" /> has been called.</returns>
    public async Task HandleAsync(IServerConnection connection)
    {
        _handled.TrySetResult(connection);
        await _release.Task;
    }

    /// <summary>
    /// Unblocks a pending <see cref="HandleAsync" /> call.
    /// </summary>
    public void Release() => _release.TrySetResult();

    /// <summary>
    /// Marks this coordinator as disposed and signals <see cref="DisposedSignal" />, so tests can pin that
    /// the <c>applicationLifetime.ApplicationStopping.Register(_coordinator.Dispose)</c> hookup fired.
    /// </summary>
    public void Dispose()
    {
        Disposed = true;
        _disposed.TrySetResult();
    }
}
