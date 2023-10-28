using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Server.Handlers;

public interface IRequestResponseHandler<TAction, TRequest, TResponse> : IHandlerBase<TAction>
    where TAction : struct, Enum
{
    Task<IStatusResult<OperationStatus, TResponse>> HandleAsync(TRequest request, CancellationToken ct);
}
