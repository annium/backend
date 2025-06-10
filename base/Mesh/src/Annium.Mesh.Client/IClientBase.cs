using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Logging;

namespace Annium.Mesh.Client;

/// <summary>
/// Base interface for mesh client functionality including communication operations
/// </summary>
public interface IClientBase : ILogSubject
{
    /// <summary>
    /// Creates an observable stream for listening to broadcast/push notifications of the specified type
    /// </summary>
    /// <typeparam name="TNotification">The type of notifications to listen for</typeparam>
    /// <returns>An observable stream of notifications</returns>
    IObservable<TNotification> Listen<TNotification>();

    // // event
    // void Notify<TEvent>(
    //     TEvent ev
    // )
    //     where TEvent : EventBaseObsolete;

    /// <summary>
    /// Sends a request without expecting a response
    /// </summary>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status result</returns>
    Task<IStatusResult<OperationStatus>> SendAsync(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    );

    /// <summary>
    /// Sends a request and expects a typed response
    /// </summary>
    /// <typeparam name="TData">The expected response data type</typeparam>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status and response data</returns>
    Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    )
        where TData : notnull;

    /// <summary>
    /// Sends a request and expects a typed response with a default value fallback
    /// </summary>
    /// <typeparam name="TData">The expected response data type</typeparam>
    /// <param name="version">The API version</param>
    /// <param name="action">The action to perform</param>
    /// <param name="request">The request object</param>
    /// <param name="defaultValue">The default value to return if the request fails</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A task containing the operation status and response data or default value</returns>
    Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        TData defaultValue,
        CancellationToken ct = default
    )
        where TData : notnull;

    // // subscription
    // Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
    //     TInit request,
    //     CancellationToken ct = default
    // )
    //     where TInit : SubscriptionInitRequestBaseObsolete;
}
