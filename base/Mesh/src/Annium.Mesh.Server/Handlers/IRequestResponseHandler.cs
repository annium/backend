using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IRequestResponseHandler<TAction, TRequest, TResponse> : IHandlerBase<TAction>
    where TAction : struct, Enum
{
    Task<IStatusResult<OperationStatus, TResponse>> HandleAsync(
        IRequestContext<TRequest> request,
        CancellationToken ct
    );
}