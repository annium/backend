using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Collections.Generic;
using Annium.Data.Operations;
using Annium.Logging;
using Annium.Mesh.Domain;
using Annium.Mesh.Serialization.Abstractions;
using Annium.Mesh.Transport.Abstractions;

namespace Annium.Mesh.Client.Internal;

internal abstract class ClientBase : IClientBase
{
    public ILogger Logger { get; }
    private readonly ISendingReceivingConnection _connection;
    private readonly ISerializer _serializer;
    private readonly IClientConfiguration _configuration;
    private readonly DisposableBox _disposable;
    private readonly ExpiringDictionary<Guid, RequestFuture> _requestFutures;
    private readonly ConcurrentDictionary<Guid, Subscription> _subscriptions = new();
    private readonly IObservable<Message> _messages;
    private bool _isDisposed;

    protected ClientBase(
        ISendingReceivingConnection connection,
        ITimeProvider timeProvider,
        ISerializer serializer,
        IClientConfiguration configuration,
        ILogger logger
    )
    {
        Logger = logger;
        _connection = connection;
        _serializer = serializer;
        _configuration = configuration;

        this.Trace("start");
        _disposable = Disposable.Box(logger);
        _requestFutures = new ExpiringDictionary<Guid, RequestFuture>(timeProvider);
        _messages = _connection.Observe().Select(serializer.DeserializeMessage).Publish().RefCount();
        _disposable += _messages
            .Where(x => x.Type is MessageType.ConnectionReady)
            .Subscribe(_ => HandleConnectionReady());
        _disposable += _messages
            .Where(x => x.Type is MessageType.Response or MessageType.SubscriptionConfirm)
            .SubscribeOn(TaskPoolScheduler.Default)
            .Subscribe(HandleResponseMessage);
        this.Trace("done");
    }

    // broadcast
    public IObservable<TNotification> Listen<TNotification>()
    {
        return _messages
            .Where(x => x.Type is MessageType.Push)
            .Select(x =>
            {
                this.Trace<Message, string>("try parse {message} as {type}", x, typeof(TNotification).FriendlyName());
                return _serializer.DeserializeData(typeof(TNotification), x.Data);
            })!
            .OfType<TNotification>()
            .SubscribeOn(TaskPoolScheduler.Default);
    }

    // // event
    // public void Notify<TEvent>(
    //     TEvent ev
    // )
    //     where TEvent : EventBaseObsolete
    // {
    //     Task.Run(() => SendInternal(ev)).ConfigureAwait(false);
    // }

    // request -> void
    public Task<IStatusResult<OperationStatus>> SendAsync(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    )
    {
        return FetchInternal(version, action, request, ct);
    }

    // request -> response
    public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct = default
    )
        where TData : notnull
    {
        return FetchInternal(version, action, request, default(TData)!, ct);
    }

    // request -> response with default value
    public Task<IStatusResult<OperationStatus, TData?>> FetchAsync<TData>(
        ushort version,
        Enum action,
        object request,
        TData defaultValue,
        CancellationToken ct = default
    )
        where TData : notnull
    {
        return FetchInternal(version, action, request, defaultValue, ct);
    }

    // // init subscription
    // public async Task<IStatusResult<OperationStatus, IObservable<TMessage>>> SubscribeAsync<TInit, TMessage>(
    //     TInit request,
    //     CancellationToken ct = default
    // )
    //     where TInit : SubscriptionInitRequestBaseObsolete
    // {
    //     var type = typeof(TInit).FriendlyName();
    //     var subscriptionId = request.Rid;
    //
    //     this.Trace("{type}#{subId} - start, create observable", type, subscriptionId);
    //     var channel = Channel.CreateUnbounded<TMessage>();
    //     var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    //
    //     async void OnDisposed()
    //     {
    //         if (!_subscriptions.TryRemove(subscriptionId, out _))
    //         {
    //             this.Trace("{type}#{subId} - skipped disposal of untracked subscription", type, subscriptionId);
    //             return;
    //         }
    //
    //         this.Trace("{type}#{subId} - unsubscribe on server", type, subscriptionId);
    //         await FetchInternal(SubscriptionCancelRequestObsolete.New(subscriptionId), CancellationToken.None);
    //         this.Trace("{type}#{subId} - unsubscribed on server", type, subscriptionId);
    //     }
    //
    //     this.Trace("{type}#{subId} - create observable", type, subscriptionId);
    //     _messageObservable
    //         .OfType<SubscriptionMessageObsolete<TMessage>>()
    //         .Where(x => x.SubscriptionId == subscriptionId)
    //         .Select(x => x.Message)
    //         .WriteToChannel(channel.Writer, cts.Token);
    //
    //     this.Trace("{type}#{subId} - init", type, subscriptionId);
    //     var response = await FetchInternal(request, Guid.Empty, ct);
    //     if (response.HasErrors)
    //     {
    //         this.Trace("{type}#{subId} - failed: {response}", type, subscriptionId, response);
    //         cts.Cancel();
    //         return Result.Status(response.Status, Observable.Empty<TMessage>()).Join(response);
    //     }
    //
    //     this.Trace("{type}#{subId} - subscribe", type, subscriptionId);
    //     var observable = ObservableExt.FromChannel(channel.Reader, OnDisposed)
    //         .ObserveOn(TaskPoolScheduler.Default);
    //
    //     this.Trace("{type}#{subId} - track observable", type, subscriptionId);
    //     if (!_subscriptions.TryAdd(subscriptionId, new(cts, (IObservable<object>)observable)))
    //         throw new InvalidOperationException($"Subscription {subscriptionId} is already tracked");
    //
    //     return Result.Status(response.Status, observable);
    // }

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
        HandleDispose();

        _isDisposed = true;

        this.Trace("done");
    }

    protected abstract void HandleDispose();

    protected abstract void HandleConnectionReady();

    private async Task<IStatusResult<OperationStatus>> FetchInternal(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct
    )
    {
        var (result, response) = await FetchRaw<IStatusResult<OperationStatus>>(version, action, request, ct);

        return response ?? result;
    }

    private async Task<IStatusResult<OperationStatus, TData?>> FetchInternal<TData>(
        ushort version,
        Enum action,
        object request,
        TData defaultValue,
        CancellationToken ct
    )
        where TData : notnull
    {
        var (result, response) = await FetchRaw<IStatusResult<OperationStatus, TData?>>(version, action, request, ct);

        return response ?? Result.Status<OperationStatus, TData?>(result.Status, defaultValue).Join(result);
    }

    private async Task<(IStatusResult<OperationStatus>, TResponse?)> FetchRaw<TResponse>(
        ushort version,
        Enum action,
        object request,
        CancellationToken ct
    )
        where TResponse : notnull
    {
        var id = Guid.NewGuid();
        var tcs = new TaskCompletionSource<object>();
        using var cts = new CancellationTokenSource(_configuration.ResponseTimeout.ToTimeSpan());
        // external token - operation canceled
        ct.Register(() =>
        {
            // if not arrived and not expired - cancel
            if (!tcs.Task.IsCompleted && !cts.IsCancellationRequested)
            {
                this.Trace("request {id} - cancel operation", id);
                cts.Cancel();
                tcs.TrySetException(new OperationCanceledException(ct));
            }
        });
        cts.Token.Register(() =>
        {
            // if not arrived and not canceled - expire
            if (!tcs.Task.IsCompleted && !ct.IsCancellationRequested)
            {
                this.Trace("request {id} - cancel by timeout", id);
                tcs.TrySetException(new TimeoutException());
            }
        });

        _requestFutures.Add(id, new RequestFuture(tcs, cts, typeof(TResponse)), _configuration.ResponseTimeout);

        try
        {
            var data = _serializer.SerializeData(request.GetType(), request);
            var message = new Message
            {
                Id = id,
                Version = version,
                Type = MessageType.Request,
                Action = Convert.ToInt32(action),
                Data = data
            };
            if (!await SendInternal(message))
                return (Result.Status(OperationStatus.NetworkError).Error("Socket is closed"), default);

            var response = (TResponse)await tcs.Task;
            return (Result.Status(OperationStatus.Ok), response);
        }
        catch (OperationCanceledException)
        {
            return (Result.Status(OperationStatus.Aborted), default);
        }
        catch (TimeoutException)
        {
            return (Result.Status(OperationStatus.Timeout).Error("Operation timed out"), default);
        }
        catch (Exception e)
        {
            return (Result.Status(OperationStatus.UncaughtError).Error(e.Message), default);
        }
    }

    private async Task<bool> SendInternal(Message message)
    {
        this.Trace("send message {message}", message);
        var result = await _connection.SendAsync(_serializer.SerializeMessage(message), CancellationToken.None);

        return result is ConnectionSendStatus.Ok;
    }

    private void HandleResponseMessage(Message message)
    {
        if (!_requestFutures.Remove(message.Id, out var future))
            return;

        if (future.CancellationSource.IsCancellationRequested)
        {
            this.Trace("dismiss message {message} - request was cancelled", message);
            return;
        }

        var response = ParseResponseMessage(message, future.ResponseType);
        if (response is null)
        {
            this.Error("failed to parse response from message {message}", message);
            return;
        }

        this.Trace("complete future with response from message {message}", message);
        future.TaskSource.TrySetResult(response);
    }

    private object? ParseResponseMessage(Message message, Type? responseType)
    {
        this.Trace("parse message {message} to response", message);

        if (responseType is null)
        {
            this.Trace("parsing message {message} to data response failed - no response type stored with future", message);
            return null;
        }

        var response = _serializer.DeserializeData(responseType, message.Data);
        this.Trace("parsed message {message} to response {response}", message, response);

        return response;
    }

    private record struct RequestFuture(TaskCompletionSource<object> TaskSource, CancellationTokenSource CancellationSource, Type? ResponseType);

    private record struct Subscription(CancellationTokenSource Cts, IObservable<object> Observable);
}