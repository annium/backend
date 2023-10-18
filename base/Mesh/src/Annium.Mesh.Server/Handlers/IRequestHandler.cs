using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

public interface IRequestHandler<TAction, TRequest> : IHandlerBase<TAction>
    where TAction : struct, Enum
{
    Task<IStatusResult<OperationStatus>> HandleAsync(
        IRequestContext<TRequest> request,
        CancellationToken ct
    );
}