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
/// Handler that throws synchronously, used to verify a faulting request handler does not tear down the
/// connection for other in-flight or subsequent requests.
/// </summary>
internal class ThrowHandler : IRequestResponseHandler<Action, EchoRequest, string>
{
    /// <summary>
    /// Gets the version of this handler.
    /// </summary>
    public static ushort Version => 1;

    /// <summary>
    /// Gets the action type this handler responds to.
    /// </summary>
    public static Action Action => Action.Throw;

    /// <summary>
    /// Handles the request by throwing, simulating a misbehaving handler.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>Never returns normally; always throws.</returns>
    public Task<IStatusResult<OperationStatus, string>> HandleAsync(EchoRequest request, CancellationToken ct)
    {
        throw new InvalidOperationException("handler boom");
    }
}
