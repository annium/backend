using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Server.Demo;

/// <summary>
/// Handler for echo request-response operations that returns the input message back to the client.
/// </summary>
internal class EchoHandler : IRequestResponseHandler<Action, EchoRequest, string>
{
    /// <summary>
    /// Gets the version of this handler.
    /// </summary>
    public static ushort Version => 1;

    /// <summary>
    /// Gets the action type this handler responds to.
    /// </summary>
    public static Action Action => Action.Echo;

    /// <summary>
    /// Handles the echo request by returning the input message back to the client.
    /// </summary>
    /// <param name="request">The echo request containing the message to echo.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation with the echoed message.</returns>
    public Task<IStatusResult<OperationStatus, string>> HandleAsync(EchoRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Status(OperationStatus.Ok, request.Message));
    }
}
