using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Collections.Generic;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;
using Annium.Data.Operations;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using ClientWebSocket = Annium.Net.WebSockets.ClientWebSocket;
using ClientWebSocketOptions = Annium.Net.WebSockets.ClientWebSocketOptions;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class ClientBase : IClientBase, IAsyncDisposable
    {
        public bool IsConnected => _socket.State == WebSocketState.Open;
        public event Action ConnectionLost = delegate { };
        public event Action ConnectionRestored = delegate { };
        private readonly ClientConfiguration _configuration;
        private readonly Serializer _serializer;
        private readonly ClientWebSocket _socket;
        private readonly ExpiringDictionary<Guid, RequestFuture> _requestFutures;
        private readonly ConcurrentDictionary<Guid, IDisposable> _subscriptions = new();
        private readonly IObservable<AbstractResponseBase> _responseObservable;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        public ClientBase(
            ITimeProvider timeProvider,
            Serializer serializer,
            ClientConfiguration configuration
        )
        {
            _serializer = serializer;
            _configuration = configuration;

            var options = new ClientWebSocketOptions();
            if (_configuration.AutoReconnect)
            {
                options.ReconnectOnFailure = true;
                options.OnConnectionLost = OnConnectionLost;
                options.OnConnectionRestored = OnConnectionRestored;
            }

            _socket = new ClientWebSocket(options);
            _requestFutures = new ExpiringDictionary<Guid, RequestFuture>(timeProvider);
            _responseObservable = _socket.Listen().Select(_serializer.Deserialize<AbstractResponseBase>);
            _disposable += _responseObservable.OfType<ResponseBase>().Subscribe(CompleteResponse);
        }

        public Task ConnectAsync(CancellationToken ct = default) =>
            _socket.ConnectAsync(_configuration.Uri, ct);

        public Task DisconnectAsync(CancellationToken ct = default) =>
            _socket.DisconnectAsync(ct);

        public Action Listen<TNotification>(
            Action<TNotification> handle
        )
            where TNotification : NotificationBase
        {
            var subscription = _responseObservable.OfType<TNotification>().Subscribe(handle);

            return () => subscription.Dispose();
        }

        public Action Listen<TNotification>(
            Func<TNotification, Task> handle
        )
            where TNotification : NotificationBase
        {
            var subscription = _responseObservable.OfType<TNotification>().SubscribeAsync(handle);

            return () => subscription.Dispose();
        }

        public void Notify<TEvent>(
            TEvent ev
        )
            where TEvent : EventBase
        {
            Task.Run(() => SendInternal(ev));
        }

        public Task<IStatusResult<OperationStatus>> SendAsync(
            RequestBase request,
            CancellationToken ct = default
        )
        {
            return FetchInternal<RequestBase, ResultResponse, IStatusResult<OperationStatus>>(request, ct,
                x => x.Result);
        }

        public Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TResponse>(
            RequestBase request,
            CancellationToken ct = default
        )
        {
            return FetchInternal<RequestBase, ResultResponse<TResponse>, IStatusResult<OperationStatus, TResponse>>(
                request, ct, x => x.Result);
        }
        //
        // public Task<IStatusResult<OperationStatus, DataStream<TResponseChunk>>> FetchStream<TRequest, TResponseChunk>(
        //     TRequest request,
        //     CancellationToken ct = default
        // )
        //     where TRequest : RequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, DataStream<TResponse, TResponseChunk>>>
        //     FetchStream<TRequest, TResponse, TResponseChunk>(
        //         TRequest request,
        //         CancellationToken ct = default
        //     )
        //     where TRequest : RequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus>> SendAsync<TRequestChunk>(
        //     DataStream<TRequestChunk> request,
        //     CancellationToken ct = default
        // )
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus>> SendAsync<TRequest, TRequestChunk>(
        //     DataStream<TRequest, TRequestChunk> request,
        //     CancellationToken ct = default
        // )
        //     where TRequest : StreamHeadRequestBase
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TRequestChunk, TResponse>(
        //     DataStream<TRequestChunk> request,
        //     CancellationToken ct = default
        // )
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TRequest, TRequestChunk, TResponse>(
        //     DataStream<TRequest, TRequestChunk> request,
        //     CancellationToken ct = default
        // )
        //     where TRequest : StreamHeadRequestBase
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, DataStream<TResponseChunk>>>
        //     FetchStream<TRequestChunk, TResponseChunk>(
        //         DataStream<TRequestChunk> request,
        //         CancellationToken ct = default
        //     )
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, DataStream<TResponse, TRequestChunk>>>
        //     FetchStream<TRequestChunk, TResponse, TResponseChunk>(
        //         DataStream<TRequestChunk> request,
        //         CancellationToken ct = default
        //     )
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, DataStream<TResponseChunk>>>
        //     FetchStream<TRequest, TRequestChunk, TResponseChunk>(
        //         DataStream<TRequest, TRequestChunk> request,
        //         CancellationToken ct = default
        //     )
        //     where TRequest : StreamHeadRequestBase
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task<IStatusResult<OperationStatus, DataStream<TResponse, TRequestChunk>>>
        //     FetchStream<TRequest, TRequestChunk, TResponse, TResponseChunk>(
        //         DataStream<TRequest, TRequestChunk> request,
        //         CancellationToken ct = default
        //     )
        //     where TRequest : StreamHeadRequestBase
        //     where TRequestChunk : StreamChunkRequestBase
        // {
        //     throw new NotImplementedException();
        // }

        public Task<IStatusResult<OperationStatus, Guid>> SubscribeAsync<TInit, TMessage>(
            TInit request,
            Action<TMessage> handle,
            CancellationToken ct = default
        )
            where TInit : SubscriptionInitRequestBase
        {
            return SubscribeInternal<TInit, TMessage>(request, o => o.Subscribe(x => handle(x.Message)), ct);
        }

        public Task<IStatusResult<OperationStatus, Guid>> SubscribeAsync<TInit, TMessage>(
            TInit request,
            Func<TMessage, Task> handle,
            CancellationToken ct = default
        )
            where TInit : SubscriptionInitRequestBase
        {
            return SubscribeInternal<TInit, TMessage>(request, o => o.Subscribe(x => handle(x.Message)), ct);
        }

        public Task<IStatusResult<OperationStatus>> UnsubscribeAsync(
            SubscriptionCancelRequest request,
            CancellationToken ct = default
        )
        {
            return FetchInternal<SubscriptionCancelRequest, ResultResponse, IStatusResult<OperationStatus>>(request, ct,
                x => x.Result);
        }

        public async ValueTask DisposeAsync()
        {
            await _disposable.DisposeAsync();
            await _socket.DisconnectAsync(CancellationToken.None);
            _socket.Dispose();
        }

        private Task OnConnectionLost()
        {
            ConnectionLost.Invoke();
            return Task.CompletedTask;
        }

        private Task OnConnectionRestored()
        {
            ConnectionRestored.Invoke();
            return Task.CompletedTask;
        }

        private async Task SendInternal<T>(T data)
            where T : notnull
        {
            await _socket.SendWith(data, _serializer, CancellationToken.None);
        }

        private async Task<TResponseData> FetchInternal<TRequest, TResponse, TResponseData>(
            TRequest request,
            CancellationToken ct,
            Func<TResponse, TResponseData> getData
        )
            where TRequest : RequestBase
            where TResponse : ResponseBase
        {
            var tcs = new TaskCompletionSource<ResponseBase>();
            var cts = new CancellationTokenSource();
            ct.Register(() =>
            {
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                    tcs.SetException(new OperationCanceledException(ct));
                }
            });

            _requestFutures.Add(request.Rid, new RequestFuture(tcs, cts), _configuration.ResponseLifetime);
            await SendInternal(request);
            var response = (TResponse) await tcs.Task;
            var data = getData(response);

            return data;
        }

        private async Task<IStatusResult<OperationStatus, Guid>> SubscribeInternal<TInit, TMessage>(
            TInit request,
            Func<IObservable<SubscriptionMessage<TMessage>>, IDisposable> subscribe,
            CancellationToken ct
        )
            where TInit : SubscriptionInitRequestBase
        {
            var response =
                await FetchInternal<TInit, ResultResponse<Guid>, IStatusResult<OperationStatus, Guid>>(request, ct,
                    x => x.Result);
            if (response.HasErrors)
                return Result.Status(response.Status, Guid.Empty).Join(response);

            var subscriptionId = response.Data;
            _subscriptions.TryAdd(
                subscriptionId,
                subscribe(
                    _responseObservable
                        .OfType<SubscriptionMessage<TMessage>>()
                        .Where(x => x.SubscriptionId == subscriptionId)
                )
            );

            return Result.Status(response.Status, subscriptionId);
        }

        private void CompleteResponse(ResponseBase response)
        {
            if (
                !_requestFutures.Remove(response.Rid, out var future) ||
                future.CancellationSource.IsCancellationRequested
            )
                return;

            future.CancellationSource.Cancel();
            future.TaskSource.SetResult(response);
        }

        private record RequestFuture(TaskCompletionSource<ResponseBase> TaskSource,
            CancellationTokenSource CancellationSource);
    }
}