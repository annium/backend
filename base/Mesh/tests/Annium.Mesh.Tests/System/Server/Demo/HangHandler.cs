using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server;
using Annium.Mesh.Tests.System.Domain;
using Action = Annium.Mesh.Tests.System.Domain.Action;

namespace Annium.Mesh.Tests.System.Server.Demo;

/// <summary>
/// Handler that never produces a response until its cancellation token fires, used to exercise caller-side
/// cancellation and response-timeout paths on the client.
/// </summary>
internal class HangHandler : IRequestResponseHandler<Action, EchoRequest, string>
{
    /// <summary>
    /// Gets the version of this handler.
    /// </summary>
    public static ushort Version => 1;

    /// <summary>
    /// Gets the action type this handler responds to.
    /// </summary>
    public static Action Action => Action.Hang;

    /// <summary>
    /// Handles the request by delaying until cancelled, so the client never receives a timely response.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="ct">The cancellation token (fires on connection teardown).</param>
    /// <returns>A task that only completes once the token is cancelled.</returns>
    public async Task<IStatusResult<OperationStatus, string>> HandleAsync(EchoRequest request, CancellationToken ct)
    {
        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (OperationCanceledException)
        {
            // connection teardown — expected; fall through and return.
        }

        return Result.Status(OperationStatus.Ok, request.Message);
    }
}
