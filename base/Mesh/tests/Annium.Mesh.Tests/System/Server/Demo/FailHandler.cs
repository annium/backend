using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Server.Demo;

/// <summary>
/// Handler that always returns a non-Ok status with an error, used to verify error-status round trips to the client.
/// </summary>
internal class FailHandler : IRequestResponseHandler<Action, EchoRequest, string>
{
    /// <summary>
    /// Gets the version of this handler.
    /// </summary>
    public static ushort Version => 1;

    /// <summary>
    /// Gets the action type this handler responds to.
    /// </summary>
    public static Action Action => Action.Fail;

    /// <summary>
    /// Handles the request by returning a NotFound status carrying an error message.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task with a non-Ok status result.</returns>
    public Task<IStatusResult<OperationStatus, string>> HandleAsync(EchoRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Status(OperationStatus.NotFound, string.Empty).Error("not found"));
    }
}
