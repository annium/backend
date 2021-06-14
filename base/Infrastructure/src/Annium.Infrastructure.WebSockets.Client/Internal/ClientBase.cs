using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Reactive.Concurrency;
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
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Client.Internal
{
    internal abstract class ClientBase<TSocket> : IClientBase, ILogSubject
        where TSocket : ISendingReceivingWebSocket
    {
        public bool IsConnected => Socket.State == WebSocketState.Open;
        public ILogger Logger { get; }
        protected TSocket Socket { get; }
        private readonly Serializer _serializer;
        private readonly IClientConfigurationBase _configuration;
        private readonly ExpiringDictionary<Guid, RequestFuture> _requestFutures;
        private readonly ConcurrentDictionary<Guid, IDisposable> _subscriptions = new();
        private readonly IObservable<AbstractResponseBase> _responseObservable;
        private readonly AsyncDisposableBox _disposable = Disposable.AsyncBox();

        protected ClientBase(
            TSocket socket,
            ITimeProvider timeProvider,
            Serializer serializer,
            IClientConfigurationBase configuration,
            ILogger logger
        )
        {
            Socket = socket;
            _serializer = serializer;
            _configuration = configuration;
            Logger = logger;

            _requestFutures = new ExpiringDictionary<Guid, RequestFuture>(timeProvider);
            _responseObservable = Socket.Listen().Select(_serializer.Deserialize<AbstractResponseBase>);
            _disposable += _responseObservable.OfType<ResponseBase>()
                .ObserveOn(TaskPoolScheduler.Default)
                .Subscribe(CompleteResponse);
        }

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
            Task.Run(() => SendInternal(ev)).ConfigureAwait(false);
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
                this.Log().Trace($"{typeof(TInit).FriendlyName()} - start");
                var token = CancellationTokenSource.CreateLinkedTokenSource(observeToken, ct).Token;
                request.SetId();
                var subscriptionId = request.SubscriptionId;
                var subscription = _responseObservable
                    .OfType<SubscriptionMessage<TMessage>>()
                    .Where(x => x.SubscriptionId == subscriptionId)
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(TaskPoolScheduler.Default)
                    .Select(x => x.Message)
                    .Subscribe(observer);

                this.Log().Trace($"{typeof(TInit).FriendlyName()} - init");
                var response = await FetchInternal<TInit, ResultResponse<Guid>, IStatusResult<OperationStatus, Guid>>(request, token, x => x.Result);
                if (response.HasErrors)
                {
                    this.Log().Trace($"{typeof(TInit).FriendlyName()} - failed: {response}");
                    subscription.Dispose();
                    observer.OnError(new WebSocketClientException(response));
                    return Disposable.Empty;
                }

                this.Log().Trace($"{typeof(TInit).FriendlyName()} - subscribed, sid {subscriptionId}");
                _subscriptions.TryAdd(subscriptionId, subscription);

                await token;

                this.Log().Trace($"{typeof(TInit).FriendlyName()} - unsubscribing");
                return Disposable.Create(() =>
                {
                    this.Log().Trace($"{typeof(TInit).FriendlyName()} - dispose subscription");
                    subscription.Dispose();
                    if (!_subscriptions.TryRemove(subscriptionId, out _))
                        return;

                    this.Log().Trace($"{typeof(TInit).FriendlyName()} - init unsubscribe on server");
                    FetchInternal<SubscriptionCancelRequest, ResultResponse, IStatusResult<OperationStatus>>(
                            SubscriptionCancelRequest.New(subscriptionId),
                            CancellationToken.None,
                            x => x.Result
                        )
                        .ContinueWith(
                            _ => this.Log().Trace($"{typeof(TInit).FriendlyName()} - unsubscribed on server"),
                            CancellationToken.None
                        );
                });
            }).SubscribeOn(TaskPoolScheduler.Default);

        public virtual async ValueTask DisposeAsync()
        {
            this.Log().Trace("start, dispose subscriptions");
            Parallel.ForEach(_subscriptions.Values, x => x.Dispose());
            this.Log().Trace("dispose disposable box");
            await _disposable.DisposeAsync();
            this.Log().Trace("done");
        }

        private async Task SendInternal<T>(T data)
            where T : notnull
        {
            await Socket.SendWith(data, _serializer, CancellationToken.None);
        }

        private async Task<TResponseData> FetchInternal<TRequest, TResponse, TResponseData>(
            TRequest request,
            CancellationToken ct,
            Func<TResponse, TResponseData> getData
        )
            where TRequest : AbstractRequestBase // because Subscription controls also go here
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
            this.Log().Trace($"Send request {request.Tid}#{request.Rid}");
            await SendInternal(request);
            var response = (TResponse) await tcs.Task;
            var data = getData(response);

            return data;
        }

        private void CompleteResponse(ResponseBase response)
        {
            if (
                _requestFutures.Remove(response.Rid, out var future) &&
                !future.CancellationSource.IsCancellationRequested
            )
            {
                this.Log().Trace($"Complete response {response.Tid}#{response.Rid}");
                future.TaskSource.TrySetResult(response);
            }
        }

        private record RequestFuture(TaskCompletionSource<ResponseBase> TaskSource, CancellationTokenSource CancellationSource);
    }
}