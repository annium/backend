using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;

namespace Annium.Mesh.Client;

public interface IClientBase : ILogSubject
{
    // broadcast
    IObservable<TNotification> Listen<TNotification>()
        where TNotification : NotificationBase;

    // event
    void Notify<TEvent>(
        TEvent ev
    )
        where TEvent : EventBase;

    // request -> void
    Task<IStatusResult<OperationStatus>> SendAsync(
        RequestBase request,
        CancellationToken ct = default
    );

    // request -> response
    Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TResponse>(
        RequestBase request,
        CancellationToken ct = default
    );

    // request -> response with default value
    Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TResponse>(
        RequestBase request,
        TResponse defaultValue,
        CancellationToken ct = default
    );

    // // request -> response stream
    // Task<IStatusResult<OperationStatus, DataStream<TResponseChunk>>> FetchStream<TRequest, TResponseChunk>(
    //     TRequest request,
    //     CancellationToken ct = default
    // )
    //     where TRequest : RequestBase;
    //
    // // request -> response stream
    // Task<IStatusResult<OperationStatus, DataStream<TResponse, TResponseChunk>>> FetchStream<TRequest, TResponse,
    //     TResponseChunk>(
    //     TRequest request,
    //     CancellationToken ct = default
    // )
    //     where TRequest : RequestBase;
    //
    // // request stream -> void
    // Task<IStatusResult<OperationStatus>> SendAsync<TRequestChunk>(
    //     DataStream<TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequestChunk : StreamChunkRequestBase;
    //
    // // request stream -> void
    // Task<IStatusResult<OperationStatus>> SendAsync<TRequest, TRequestChunk>(
    //     DataStream<TRequest, TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase;
    //
    // // request stream -> response
    // Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TRequestChunk, TResponse>(
    //     DataStream<TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequestChunk : StreamChunkRequestBase;
    //
    // // request stream -> response
    // Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TRequest, TRequestChunk, TResponse>(
    //     DataStream<TRequest, TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase;
    //
    // // request stream -> response stream
    // Task<IStatusResult<OperationStatus, DataStream<TResponseChunk>>> FetchStreamAsync<TRequestChunk, TResponseChunk>(
    //     DataStream<TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequestChunk : StreamChunkRequestBase;
    //
    // // request stream -> response stream
    // Task<IStatusResult<OperationStatus, DataStream<TResponse, TRequestChunk>>> FetchStreamAsync<TRequestChunk, TResponse,
    //     TResponseChunk>(
    //     DataStream<TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequestChunk : StreamChunkRequestBase;
    //
    // // request stream -> response stream
    // Task<IStatusResult<OperationStatus, DataStream<TResponseChunk>>> FetchStreamAsync<TRequest, TRequestChunk,
    //     TResponseChunk>(
    //     DataStream<TRequest, TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase;
    //
    // // request stream -> response stream
    // Task<IStatusResult<OperationStatus, DataStream<TResponse, TRequestChunk>>> FetchStreamAsync<TRequest, TRequestChunk,
    //     TResponse, TResponseChunk>(
    //     DataStream<TRequest, TRequestChunk> request,
    //     CancellationToken ct = default
    // )
    //     where TRequest : StreamHeadRequestBase
    //     where TRequestChunk : StreamChunkRequestBase;

    // init subscription
    Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
        TInit request,
        CancellationToken ct = default
    )
        where TInit : SubscriptionInitRequestBase;
}