using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Collections.Generic;
using Annium.Core.Internal;
using Annium.Core.Primitives;
using Annium.Core.Runtime.Time;
using Annium.Data.Operations;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using ClientWebSocket = Annium.Net.WebSockets.ClientWebSocket;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal class ClientBase : IClientBase
    {
        public bool IsConnected => _socket.State == WebSocketState.Open;
        public event Func<Task> ConnectionLost = () => Task.CompletedTask;
        public event Func<Task> ConnectionRestored = () => Task.CompletedTask;
        private readonly Serializer _serializer;
        private readonly ClientConfiguration _configuration;
        private readonly ClientWebSocket _socket;
        private readonly IBackgroundExecutor _executor = Executor.Background.Parallel();
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

            _socket = new ClientWebSocket(_configuration.WebSocketOptions);
            _socket.ConnectionLost += () => ConnectionLost.Invoke();
            _socket.ConnectionRestored += () => ConnectionRestored.Invoke();

            _disposable += _executor;
            _executor.Start();

            _requestFutures = new ExpiringDictionary<Guid, RequestFuture>(timeProvider);
            _responseObservable = _socket.Listen().Select(_serializer.Deserialize<AbstractResponseBase>);
            _disposable += _responseObservable.OfType<ResponseBase>()
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(CompleteResponse);
        }

        public Task ConnectAsync(CancellationToken ct = default) =>
            _socket.ConnectAsync(_configuration.Uri, ct);

        public Task DisconnectAsync(CancellationToken ct = default) =>
            _socket.DisconnectAsync(ct);

        public IObservable<TNotification> Listen<TNotification>()
            where TNotification : NotificationBase
        {
            return _responseObservable.OfType<TNotification>();
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

        public IObservable<TMessage> Listen<TInit, TMessage>(
            TInit request,
            CancellationToken ct = default
        )
            where TInit : SubscriptionInitRequestBase
            => Observable.Create<TMessage>(async (observer, observeToken) =>
            {
                var token = CancellationTokenSource.CreateLinkedTokenSource(observeToken, ct).Token;
                var subscriptionId = Guid.Empty;
                var subscription = _responseObservable
                    .OfType<SubscriptionMessage<TMessage>>()
                    // ReSharper disable once AccessToModifiedClosure
                    .Where(x => x.SubscriptionId == subscriptionId)
                    .ObserveOn(TaskPoolScheduler.Default)
                    .Select(x => x.Message)
                    .Subscribe(observer);

                var response = await FetchInternal<TInit, ResultResponse<Guid>, IStatusResult<OperationStatus, Guid>>(request, token, x => x.Result);
                if (response.HasErrors)
                {
                    subscription.Dispose();
                    observer.OnError(new WebSocketClientException(response));
                    return Disposable.Empty;
                }

                subscriptionId = response.Data;
                _subscriptions.TryAdd(subscriptionId, subscription);

                await token;

                return Disposable.Create(() =>
                {
                    this.Trace(() => "Dispose subscription");
                    subscription.Dispose();
                    if (!_subscriptions.TryRemove(subscriptionId, out _))
                        return;

                    this.Trace(() => "Init unsubscription on server");
                    FetchInternal<SubscriptionCancelRequest, ResultResponse, IStatusResult<OperationStatus>>(
                            SubscriptionCancelRequest.New(subscriptionId),
                            CancellationToken.None,
                            x => x.Result
                        )
                        .ContinueWith(_ => this.Trace(() => "Unsubscribed on server side"), CancellationToken.None);
                });
            }).SubscribeOn(TaskPoolScheduler.Default);

        public async Task<IStatusResult<OperationStatus>> UnsubscribeAsync(
            SubscriptionCancelRequest request,
            CancellationToken ct = default
        )
        {
            if (!_subscriptions.TryRemove(request.SubscriptionId, out var subscription))
                return Result.Status(OperationStatus.NotFound);

            subscription.Dispose();

            return await FetchInternal<SubscriptionCancelRequest, ResultResponse, IStatusResult<OperationStatus>>(request, ct, x => x.Result);
        }

        public async ValueTask DisposeAsync()
        {
            this.Trace(() => "start, dispose subscriptions");
            Parallel.ForEach(_subscriptions.Values, x => x.Dispose());
            this.Trace(() => "dispose disposable box");
            await _disposable.DisposeAsync();
            this.Trace(() => "disconnect socket");
            await _socket.DisconnectAsync(CancellationToken.None);
            this.Trace(() => "dispose socket");
            await _socket.DisposeAsync();
            this.Trace(() => "done");
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
            var cts = new CancellationTokenSource(_configuration.ResponseTimeout.ToTimeSpan());
            // external token - operation canceled
            ct.Register(() =>
            {
                // if not arrived and not expired - cancel
                if (!tcs.Task.IsCompleted && !cts.IsCancellationRequested)
                {
                    cts.Cancel();
                    tcs.TrySetException(new OperationCanceledException(ct));
                }
            });
            cts.Token.Register(() =>
            {
                // if not arrived and not canceled - expire
                if (!tcs.Task.IsCompleted && !ct.IsCancellationRequested)
                {
                    tcs.TrySetException(new TimeoutException());
                }
            });

            _requestFutures.Add(request.Rid, new RequestFuture(tcs, cts), _configuration.ResponseTimeout);
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
            var subscriptionId = Guid.Empty;
            var subscription = subscribe(
                _responseObservable
                    .OfType<SubscriptionMessage<TMessage>>()
                    // ReSharper disable once AccessToModifiedClosure
                    .Where(x => x.SubscriptionId == subscriptionId)
                    .ObserveOn(TaskPoolScheduler.Default)
            );

            var response = await FetchInternal<TInit, ResultResponse<Guid>, IStatusResult<OperationStatus, Guid>>(request, ct, x => x.Result);
            if (response.HasErrors)
            {
                subscription.Dispose();
                return Result.Status(response.Status, Guid.Empty).Join(response);
            }

            subscriptionId = response.Data;
            _subscriptions.TryAdd(subscriptionId, subscription);

            return Result.Status(response.Status, subscriptionId);
        }

        private void CompleteResponse(ResponseBase response)
        {
            if (
                _requestFutures.Remove(response.Rid, out var future) &&
                !future.CancellationSource.IsCancellationRequested
            )
                _executor.Schedule(() => future.TaskSource.TrySetResult(response));
        }

        private record RequestFuture(TaskCompletionSource<ResponseBase> TaskSource, CancellationTokenSource CancellationSource);
    }
}