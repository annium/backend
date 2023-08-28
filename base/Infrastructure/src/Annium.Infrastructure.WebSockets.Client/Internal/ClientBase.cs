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
using Annium.Data.Operations;
using Annium.Debug;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Logging;
using Annium.Net.WebSockets.Obsolete;
using Annium.Threading;

namespace Annium.Infrastructure.WebSockets.Client.Internal;

internal abstract class ClientBase<TSocket> : IClientBase, ILogSubject
    where TSocket : class, ISendingReceivingWebSocket
{
    public bool IsConnected => Socket.State == WebSocketState.Open;
    public ILogger Logger { get; }
    protected TSocket Socket { get; }
    private readonly Serializer _serializer;
    private readonly IClientConfigurationBase _configuration;
    private readonly ExpiringDictionary<Guid, RequestFuture> _requestFutures;
    private readonly ConcurrentDictionary<Guid, ValueTuple<CancellationTokenSource, IObservable<object>>> _subscriptions = new();
    private readonly IObservable<AbstractResponseBase> _responseObservable;
    private readonly AsyncDisposableBox _disposable;

    protected ClientBase(
        TSocket socket,
        ITimeProvider timeProvider,
        Serializer serializer,
        IClientConfigurationBase configuration,
        ILogger logger,
        ITracer tracer
    )
    {
        Socket = socket;
        _serializer = serializer;
        _configuration = configuration;
        Logger = logger;
        _disposable = Disposable.AsyncBox(tracer);

        _requestFutures = new ExpiringDictionary<Guid, RequestFuture>(timeProvider);
        _responseObservable = Socket.Listen().Select(_serializer.Deserialize<AbstractResponseBase>);
        _disposable += _responseObservable.OfType<ResponseBase>()
            .ObserveOn(TaskPoolScheduler.Default)
            .Subscribe(CompleteResponse);
    }

    // broadcast
    public IObservable<TNotification> Listen<TNotification>()
        where TNotification : NotificationBase
    {
        return _responseObservable.OfType<TNotification>();
    }

    // event
    public void Notify<TEvent>(
        TEvent ev
    )
        where TEvent : EventBase
    {
        Task.Run(() => SendInternal(ev)).ConfigureAwait(false);
    }

    // request -> void
    public Task<IStatusResult<OperationStatus>> SendAsync(
        RequestBase request,
        CancellationToken ct = default
    )
    {
        return FetchInternal(request, ct);
    }

    // request -> response
    public Task<IStatusResult<OperationStatus, TData>> FetchAsync<TData>(
        RequestBase request,
        CancellationToken ct = default
    )
    {
        return FetchInternal(request, default(TData)!, ct);
    }

    // request -> response with default value
    public Task<IStatusResult<OperationStatus, TResponse>> FetchAsync<TResponse>(
        RequestBase request,
        TResponse defaultValue,
        CancellationToken ct = default
    )
    {
        return FetchInternal(request, defaultValue, ct);
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

    // init subscription
    public async Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
        TInit request,
        CancellationToken ct = default
    )
        where TInit : SubscriptionInitRequestBase
    {
        var type = typeof(TInit).FriendlyName();
        var subscriptionId = request.Rid;
        this.Trace($"{type}#{subscriptionId} - start");

        this.Trace($"{type}#{subscriptionId} - create observable");
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var observable = ObservableExt
            .StaticAsyncInstance<TMessage>(async ctx =>
            {
                this.Trace($"{type}#{subscriptionId} - subscribe");
                var subscription = _responseObservable
                    .OfType<SubscriptionMessage<TMessage>>()
                    .Where(x => x.SubscriptionId == subscriptionId)
                    .SubscribeOn(TaskPoolScheduler.Default)
                    .ObserveOn(TaskPoolScheduler.Default)
                    .Select(x => x.Message)
                    .Subscribe(ctx);

                await ctx.Ct;

                this.Trace($"{type}#{subscriptionId} - unsubscribing");
                return async () =>
                {
                    if (!_subscriptions.TryRemove(subscriptionId, out _))
                    {
                        this.Trace($"{type}#{subscriptionId} - skipped disposal of untracked subscription");
                        return;
                    }

                    this.Trace($"{type}#{subscriptionId} - dispose subscription");
                    subscription.Dispose();

                    this.Trace($"{type}#{subscriptionId} - unsubscribe on server");

                    await FetchInternal(SubscriptionCancelRequest.New(subscriptionId), CancellationToken.None);
                    this.Trace($"{type}#{subscriptionId} - unsubscribed on server");
                };
            }, cts.Token)
            .TrackCompletion()
            .BufferUntilSubscribed();

        this.Trace($"{type}#{subscriptionId} - init");
        var response = await FetchInternal(request, Guid.Empty, ct);
        if (response.HasErrors)
        {
            this.Trace($"{type}#{subscriptionId} - failed: {response}");
            cts.Cancel();
            await observable.WhenCompleted();
            return Result.Status(response.Status, Observable.Empty<TMessage>()).Join(response);
        }

        this.Trace($"{type}#{subscriptionId} - track observable");
        if (!_subscriptions.TryAdd(subscriptionId, (cts, (IObservable<object>)observable)))
            throw new InvalidOperationException($"Subscription {subscriptionId} is already tracked");

        return Result.Status(response.Status, observable);
    }

    public virtual async ValueTask DisposeAsync()
    {
        this.Trace("start, dispose subscriptions");
        await Task.WhenAll(_subscriptions.Values.Select(async x =>
        {
            x.Item1.Cancel();
            await x.Item2.WhenCompleted();
        }));
        this.Trace("dispose disposable box");
        await _disposable.DisposeAsync();
        this.Trace("done");
    }

    private async Task<IStatusResult<OperationStatus>> FetchInternal<TRequest>(
        TRequest request,
        CancellationToken ct
    )
        where TRequest : AbstractRequestBase // because Subscription controls also go here
    {
        var (result, response) = await FetchRaw<TRequest, ResultResponse>(request, ct);

        return response?.Result ?? result;
    }

    private async Task<IStatusResult<OperationStatus, TData>> FetchInternal<TRequest, TData>(
        TRequest request,
        TData defaultValue,
        CancellationToken ct
    )
        where TRequest : AbstractRequestBase // because Subscription controls also go here
    {
        var (result, response) = await FetchRaw<TRequest, ResultResponse<TData>>(request, ct);

        return response?.Result ?? Result.Status(result.Status, defaultValue).Join(result);
    }

    private async Task<(IStatusResult<OperationStatus>, TResponse?)> FetchRaw<TRequest, TResponse>(
        TRequest request,
        CancellationToken ct
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
                this.Trace($"request {request.Rid} - cancel operation");
                cts.Cancel();
                tcs.TrySetException(new OperationCanceledException(ct));
            }
        });
        cts.Token.Register(() =>
        {
            // if not arrived and not canceled - expire
            if (!tcs.Task.IsCompleted && !ct.IsCancellationRequested)
            {
                this.Trace($"request {request.Rid} - cancel by timeout");
                tcs.TrySetException(new TimeoutException());
            }
        });

        _requestFutures.Add(request.Rid, new RequestFuture(tcs, cts), _configuration.ResponseTimeout);

        try
        {
            if (!await SendInternal(request))
                return (Result.Status(OperationStatus.NetworkError).Error("Socket is closed"), null);

            var response = (TResponse)await tcs.Task;
            return (Result.Status(OperationStatus.Ok), response);
        }
        catch (OperationCanceledException)
        {
            return (Result.Status(OperationStatus.Aborted), null);
        }
        catch (TimeoutException)
        {
            return (Result.Status(OperationStatus.Timeout).Error("Operation timed out"), null);
        }
        catch (Exception e)
        {
            return (Result.Status(OperationStatus.UncaughtError).Error(e.Message), null);
        }
    }

    private async Task<bool> SendInternal<T>(T request)
        where T : AbstractRequestBase
    {
        if (IsConnected)
        {
            this.Trace($"send request {request.Tid}#{request.Rid}");
            await Socket.SendWith(request, _serializer, CancellationToken.None);

            return true;
        }

        this.Trace($"send request {request.Tid}#{request.Rid} - skip, socket is disconnected");

        return false;
    }

    private void CompleteResponse(ResponseBase response)
    {
        if (_requestFutures.Remove(response.Rid, out var future))
        {
            if (!future.CancellationSource.IsCancellationRequested)
            {
                this.Trace($"complete response {response.Tid}#{response.Rid}");
                future.TaskSource.TrySetResult(response);
            }
            else
                this.Trace($"dismiss cancelled response {response.Tid}#{response.Rid}");
        }
        else
            this.Trace($"dismiss unknown response {response.Tid}#{response.Rid}");
    }

    private record struct RequestFuture(TaskCompletionSource<ResponseBase> TaskSource, CancellationTokenSource CancellationSource);
}