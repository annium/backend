using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;

namespace Annium.Mesh.Server.Handlers;

/// <summary>
/// Defines a handler for processing requests that return both a status result and response data.
/// </summary>
/// <typeparam name="TAction">The enum type representing the action for this request-response handler.</typeparam>
/// <typeparam name="TRequest">The type of request to handle.</typeparam>
/// <typeparam name="TResponse">The type of response to return.</typeparam>
public interface IRequestResponseHandler<TAction, TRequest, TResponse> : IHandlerBase<TAction>
    where TAction : struct, Enum
{
    /// <summary>
    /// Handles the request asynchronously and returns both a status result and response data.
    /// </summary>
    /// <param name="request">The request data to process.</param>
    /// <param name="ct">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous request handling operation with a status and response result.</returns>
    Task<IStatusResult<OperationStatus, TResponse>> HandleAsync(TRequest request, CancellationToken ct);
}
