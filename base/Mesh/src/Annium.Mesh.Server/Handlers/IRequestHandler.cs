using Annium.Architecture.Base;
using Annium.Core.Mediator;
using Annium.Data.Operations;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Server.Models;

namespace Annium.Mesh.Server.Handlers;

// request -> void
public interface IRequestHandler<TRequest> :
    IFinalRequestHandler<IRequestContext<TRequest>, IStatusResult<OperationStatus>>
    where TRequest : RequestBase

{
}

// request -> response
public interface IRequestResponseHandler<TRequest, TResponse> :
    IFinalRequestHandler<IRequestContext<TRequest>, IStatusResult<OperationStatus, TResponse>>
    where TRequest : RequestBase
{
}