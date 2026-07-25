using System.Threading.Tasks;

namespace Annium.AspNetCore.IntegrationTesting.Tests;

/// <summary>
/// Test-only gate that lets a test hold a server-side request in flight until it explicitly chooses to let
/// it proceed, via <see cref="TaskCompletionSource" /> signals rather than a fixed delay. Registered as a
/// singleton by <see cref="SlowRequestTestHost" /> and consumed by <see cref="SlowRequestStartupFilter" />'s
/// blocking endpoint.
/// </summary>
public sealed class RequestGate
{
    /// <summary>
    /// Completes once the blocking endpoint has started handling a request and is awaiting <see cref="Release" />.
    /// </summary>
    public Task Started => _started.Task;

    /// <summary>
    /// Signals that the blocking endpoint has started handling a request.
    /// </summary>
    private readonly TaskCompletionSource _started = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Signals that the blocking endpoint should stop waiting and complete the request.
    /// </summary>
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Marks <see cref="Started" /> as finished, then waits for <see cref="Release" /> to be called.
    /// </summary>
    /// <returns>A task that completes once <see cref="Release" /> has been called.</returns>
    public Task WaitForReleaseAsync()
    {
        _started.TrySetResult();
        // VSTHRD003: _release is this gate's own TCS, completed by Release() — not alien work.
#pragma warning disable VSTHRD003
        return _release.Task;
#pragma warning restore VSTHRD003
    }

    /// <summary>
    /// Lets a pending <see cref="WaitForReleaseAsync" /> call complete.
    /// </summary>
    public void Release() => _release.TrySetResult();
}
