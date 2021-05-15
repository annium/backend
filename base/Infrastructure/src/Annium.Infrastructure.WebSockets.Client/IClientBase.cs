using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;

namespace Annium.Infrastructure.WebSockets.Client
{
    public interface IClientBase
    {
        // management
        bool IsConnected { get; }
        event Func<Task> ConnectionLost;
        event Func<Task> ConnectionRestored;
        Task ConnectAsync(CancellationToken ct = default);
        Task DisconnectAsync(CancellationToken ct = default);

        // broadcast
        IObservable<TNotification> Listen<TNotification>()
            where TNotification : NotificationBase;

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
        Task<IStatusResult<OperationStatus, Guid>> SubscribeAsync<TInit, TMessage>(
            TInit request,
            Action<TMessage> handle,
            CancellationToken ct = default
        )
            where TInit : SubscriptionInitRequestBase;

        Task<IStatusResult<OperationStatus, Guid>> SubscribeAsync<TInit, TMessage>(
            TInit request,
            Func<TMessage, Task> handle,
            CancellationToken ct = default
        )
            where TInit : SubscriptionInitRequestBase;

        // cancel subscription
        Task<IStatusResult<OperationStatus>> UnsubscribeAsync(
            SubscriptionCancelRequest request,
            CancellationToken ct = default
        );
    }
}