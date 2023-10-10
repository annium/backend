using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Collections.Generic;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Domain.Requests;
using Annium.Mesh.Domain.Responses;
using Annium.Mesh.Transport.Abstractions;
using Annium.Serialization.Abstractions;

namespace Annium.Mesh.Client.Internal;

internal abstract class ClientBase : IClientBase, ILogSubject
{
    public ILogger Logger { get; }
    private readonly ISendingReceivingConnection _connection;
    private readonly ISerializer<ReadOnlyMemory<byte>> _serializer;
    private readonly IClientConfiguration _configuration;
    private readonly DisposableBox _disposable;
    private readonly ExpiringDictionary<Guid, RequestFuture> _requestFutures;
    private readonly ConcurrentDictionary<Guid, Subscription> _subscriptions = new();
    private readonly IObservable<AbstractResponseBase> _responseObservable;
    private bool _isDisposed;

    protected ClientBase(
        ISendingReceivingConnection connection,
        ITimeProvider timeProvider,
        ISerializer<ReadOnlyMemory<byte>> serializer,
        IClientConfiguration configuration,
        ILogger logger
    )
    {
        Logger = logger;
        _connection = connection;
        _serializer = serializer;
        _configuration = configuration;
        _disposable = Disposable.Box(logger);

        _requestFutures = new ExpiringDictionary<Guid, RequestFuture>(timeProvider);

        _responseObservable = _connection.Observe().Select(serializer.Deserialize<AbstractResponseBase>);
        _disposable += _responseObservable.OfType<ResponseBase>()
            .SubscribeOn(TaskPoolScheduler.Default)
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

    // init subscription
    public async Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
        TInit request,
        CancellationToken ct = default
    )
        where TInit : SubscriptionInitRequestBase
    {
        var type = typeof(TInit).FriendlyName();
        var subscriptionId = request.Rid;

        this.Trace("{type}#{subscriptionId} - start, create observable", type, subscriptionId);
        var channel = Channel.CreateUnbounded<TMessage>();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        async void OnDisposed()
        {
            if (!_subscriptions.TryRemove(subscriptionId, out _))
            {
                this.Trace("{type}#{subscriptionId} - skipped disposal of untracked subscription", type, subscriptionId);
                return;
            }

            this.Trace("{type}#{subscriptionId} - unsubscribe on server", type, subscriptionId);
            await FetchInternal(SubscriptionCancelRequest.New(subscriptionId), CancellationToken.None);
            this.Trace("{type}#{subscriptionId} - unsubscribed on server", type, subscriptionId);
        }

        this.Trace("{type}#{subscriptionId} - create observable", type, subscriptionId);
        _responseObservable
            .OfType<SubscriptionMessage<TMessage>>()
            .Where(x => x.SubscriptionId == subscriptionId)
            .Select(x =>
            {
                this.Trace("GOT!: {x}", x.Message);
                return x.Message;
            })
            .WriteToChannel(channel.Writer, cts.Token);

        this.Trace("{type}#{subscriptionId} - init", type, subscriptionId);
        var response = await FetchInternal(request, Guid.Empty, ct);
        if (response.HasErrors)
        {
            this.Trace("{type}#{subscriptionId} - failed: {response}", type, subscriptionId, response);
            cts.Cancel();
            return Result.Status(response.Status, Observable.Empty<TMessage>()).Join(response);
        }

        this.Trace("{type}#{subscriptionId} - subscribe", type, subscriptionId);
        var observable = ObservableExt.FromChannel(channel.Reader, OnDisposed)
            .ObserveOn(TaskPoolScheduler.Default);

        this.Trace("{type}#{subscriptionId} - track observable", type, subscriptionId);
        if (!_subscriptions.TryAdd(subscriptionId, new(cts, (IObservable<object>)observable)))
            throw new InvalidOperationException($"Subscription {subscriptionId} is already tracked");

        return Result.Status(response.Status, observable);
    }

    public async ValueTask DisposeAsync()
    {
        this.Trace("start");

        if (_isDisposed)
        {
            this.Trace("already disposed");
            return;
        }

        this.Trace("start, dispose subscriptions");
        await Task.WhenAll(_subscriptions.Values.Select(async x =>
        {
            x.Cts.Cancel();
            await x.Observable.WhenCompleted(Logger);
        }));

        this.Trace("dispose disposable box");
        await _disposable.DisposeAsync();

        this.Trace("handle disposal in inherited class");
        await HandleDisposeAsync();

        _isDisposed = true;

        this.Trace("done");
    }

    protected abstract ValueTask HandleDisposeAsync();

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
                this.Trace("request {requestId} - cancel operation", request.Rid);
                cts.Cancel();
                tcs.TrySetException(new OperationCanceledException(ct));
            }
        });
        cts.Token.Register(() =>
        {
            // if not arrived and not canceled - expire
            if (!tcs.Task.IsCompleted && !ct.IsCancellationRequested)
            {
                this.Trace("request {requestId} - cancel by timeout", request.Rid);
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
        this.Trace("send request {requestType}#{requestId}", request.Tid, request.Rid);
        var result = await _connection.SendAsync(_serializer.Serialize(request), CancellationToken.None);

        return result is ConnectionSendStatus.Ok;
    }

    private void CompleteResponse(ResponseBase response)
    {
        if (_requestFutures.Remove(response.Rid, out var future))
        {
            if (!future.CancellationSource.IsCancellationRequested)
            {
                this.Trace("complete response {responseType}#{responseId}", response.Tid, response.Rid);
                future.TaskSource.TrySetResult(response);
            }
            else
                this.Trace("dismiss cancelled response {responseType}#{responseId}", response.Tid, response.Rid);
        }
        else
            this.Trace("dismiss unknown response {responseType}#{responseId}", response.Tid, response.Rid);
    }

    private record struct RequestFuture(TaskCompletionSource<ResponseBase> TaskSource, CancellationTokenSource CancellationSource);

    private record struct Subscription(CancellationTokenSource Cts, IObservable<object> Observable);
}