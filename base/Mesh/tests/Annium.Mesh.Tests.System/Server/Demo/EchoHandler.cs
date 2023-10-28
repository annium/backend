using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Server.Demo;

internal class EchoHandler : IRequestResponseHandler<Action, EchoRequest, string>
{
    public static ushort Version => 1;
    public static Action Action => Action.Echo;

    public Task<IStatusResult<OperationStatus, string>> HandleAsync(EchoRequest request, CancellationToken ct)
    {
        return Task.FromResult(Result.Status(OperationStatus.Ok, request.Message));
    }
}
