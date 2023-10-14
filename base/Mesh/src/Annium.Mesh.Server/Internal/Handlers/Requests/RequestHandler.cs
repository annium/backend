using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Server.Internal.Responses;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Internal.Handlers.Requests;

internal class RequestHandler<TRequest, TState> :
    IPipeRequestHandler<IRequestContext<TRequest, TState>, IRequestContext<TRequest, TState>, IStatusResult<OperationStatus>, ResultResponse>
    where TRequest : RequestBase
    where TState : ConnectionStateBase
{
    public async Task<ResultResponse> HandleAsync(
        IRequestContext<TRequest, TState> request,
        CancellationToken ct,
        Func<IRequestContext<TRequest, TState>, CancellationToken, Task<IStatusResult<OperationStatus>>> next
    )
    {
        var result = await next(request, ct);

        return Response.Result(request.Request.Rid, result);
    }
}