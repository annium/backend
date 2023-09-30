using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.AspNetCore.TestServer.Requests;
using Annium.Data.Operations;
using Annium.Mesh.Server.Handlers;
using Annium.Mesh.Server.Models;

namespace Annium.AspNetCore.TestServer.Handlers.Demo;

internal class EchoHandler : IRequestResponseHandler<EchoRequest, string, ConnectionState>
{
    public Task<IStatusResult<OperationStatus, string>> HandleAsync(
        IRequestContext<EchoRequest, ConnectionState> request,
        CancellationToken ct
    )
    {
        return Task.FromResult(Result.Status(OperationStatus.Ok, request.Request.Message));
    }
}