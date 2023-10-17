using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Models;
using Annium.Mesh.Tests.System.Domain;

namespace Annium.Mesh.Tests.System.Server.Demo;

internal class EchoHandler : IRequestResponseHandler<EchoRequest, string>
{
    public Task<IStatusResult<OperationStatus, string>> HandleAsync(
        IRequestContext<EchoRequest> request,
        CancellationToken ct
    )
    {
        return Task.FromResult(Result.Status(OperationStatus.Ok, request.Request.Message));
    }
}