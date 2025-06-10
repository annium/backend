using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Server.Handlers;

/// <summary>
/// Defines a handler for processing requests that return only a status result without response data.
/// </summary>
/// <typeparam name="TAction">The enum type representing the action for this request handler.</typeparam>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
public interface IRequestHandler<TAction, TRequest> : IHandlerBase<TAction>
    where TAction : struct, Enum
{
    /// <summary>
    /// Handles the request asynchronously and returns a status result.
    /// </summary>
    /// <param name="request">The request data to process.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous request handling operation with a status result.</returns>
    Task<IStatusResult<OperationStatus>> HandleAsync(TRequest request, CancellationToken ct);
}
