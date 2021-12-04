using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Responses;
using Annium.Infrastructure.WebSockets.Server.Models;

namespace Annium.Infrastructure.WebSockets.Server.Internal.Handlers.Requests;

internal class RequestResponseHandler<TRequest, TResponse, TState> :
    IPipeRequestHandler<IRequestContext<TRequest, TState>, IRequestContext<TRequest, TState>, IStatusResult<OperationStatus, TResponse>, ResultResponse<TResponse>>
    where TRequest : RequestBase
    where TState : ConnectionStateBase
{
    public async Task<ResultResponse<TResponse>> HandleAsync(
        IRequestContext<TRequest, TState> request,
        CancellationToken ct,
        Func<IRequestContext<TRequest, TState>, CancellationToken, Task<IStatusResult<OperationStatus, TResponse>>> next
    )
    {
        var result = await next(request, ct);

        return Response.Result(request.Request.Rid, result);
    }
}