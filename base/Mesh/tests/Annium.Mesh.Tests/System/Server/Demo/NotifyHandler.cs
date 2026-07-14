using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Server.Demo;

/// <summary>
/// No-response-data request handler (IRequestHandler), used to exercise the client SendAsync path that
/// returns only a status result.
/// </summary>
internal class NotifyHandler : IRequestHandler<Action, EchoRequest>
{
    /// <summary>
    /// Gets the version of this handler.
    /// </summary>
    public static ushort Version => 1;

    /// <summary>
    /// Gets the action type this handler responds to.
    /// </summary>
    public static Action Action => Action.Notify;

    /// <summary>
    /// Handles the request and returns an Ok status with no response data.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task with an Ok status result.</returns>
    public Task<IStatusResult<OperationStatus>> HandleAsync(EchoRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Status(OperationStatus.Ok));
    }
}
