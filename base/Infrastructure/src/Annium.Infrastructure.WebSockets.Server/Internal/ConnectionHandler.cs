using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class ConnectionHandler<TState> : IAsyncDisposable, ILogSubject<ConnectionHandler<TState>>
    where TState : ConnectionStateBase
{
    public ILogger<ConnectionHandler<TState>> Logger { get; }
    private readonly IServiceProvider _sp;
    private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;
    private readonly Connection _cn;
    private readonly Serializer _serializer;
    private readonly TState _state;

    public ConnectionHandler(
        IServiceProvider sp,
        Func<Guid, TState> stateFactory,
        IEnumerable<IConnectionBoundStore> connectionBoundStores,
        Connection cn,
        Serializer serializer,
        ILogger<ConnectionHandler<TState>> logger
    )
    {
        _sp = sp;
        _connectionBoundStores = connectionBoundStores;
        _cn = cn;
        _serializer = serializer;
        Logger = logger;
        _state = stateFactory(cn.Id);
    }

    public async Task HandleAsync(CancellationToken ct)
    {
        var cnId = _cn.Id;
        this.Log().Trace($"cn {cnId} - start");
        await using var scope = _sp.CreateAsyncScope();
        var executor = Executor.Background.Parallel<ConnectionHandler<TState>>();
        var lifeCycleCoordinator = scope.ServiceProvider.Resolve<LifeCycleCoordinator<TState>>();
        var pusherCoordinator = scope.ServiceProvider.Resolve<PusherCoordinator<TState>>();
        try
        {
            var tcs = new TaskCompletionSource();
            tcs.Task.ContinueWith(_ => this.Log().Trace($"cn {cnId} - listen ended"), CancellationToken.None).GetAwaiter();

            // immediately subscribe to cancellation
            ct.Register(() =>
            {
                this.Log().Trace($"cn {cnId} - complete tcs due to cancellation");
                tcs.TrySetResult();
            });

            // use derived cts for socket subscription
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // start listening to messages and adding them to scheduler
            this.Log().Trace($"cn {cnId} - init subscription");
            _cn.Socket
                .Listen()
                .Subscribe(
                    x => executor.TrySchedule(async () =>
                    {
                        this.Log().Trace($"cn {cnId} - handle message");
                        await using var messageScope = _sp.CreateAsyncScope();
                        var handler = messageScope.ServiceProvider.Resolve<MessageHandler<TState>>();
                        await handler.HandleMessage(_cn.Socket, _state, x);
                    }),
                    e =>
                    {
                        this.Log().Trace($"cn {cnId} - handle error");
                        cts.Cancel();
                        this.Log().Error(e);
                        this.Log().Trace($"cn {cnId} - complete tcs due to error");
                        tcs.TrySetResult();
                    },
                    () =>
                    {
                        this.Log().Trace($"cn {cnId} - complete tcs due to socket closed");
                        tcs.TrySetResult();
                    },
                    cts.Token
                );

            // execute start hook
            this.Log().Trace($"cn {cnId} - handle lifecycle start - start");
            await lifeCycleCoordinator.StartAsync(_state);
            this.Log().Trace($"cn {cnId} - handle lifecycle start - done");

            // notify client, that connection is ready
            this.Log().Trace($"cn {cnId} - notify connection ready");
            _cn.Socket.SendWith(new ConnectionReadyNotification(), _serializer, cts.Token).Subscribe();

            // execute run hook
            var pusherTask = pusherCoordinator.RunAsync(_state, cts.Token);
            pusherTask.ContinueWith(_ => this.Log().Trace($"cn {cnId} - push ended"), CancellationToken.None).GetAwaiter();
            this.Log().Trace($"cn {cnId} - push handlers start - start");
            this.Log().Trace($"cn {cnId} - push handlers start - done");

            // start scheduler to process backlog and run upcoming work immediately
            this.Log().Trace($"cn {cnId} - start executor");
            executor.Start(CancellationToken.None);

            // wait until connection complete
            this.Log().Trace($"cn {cnId} - wait until connection complete");
            await Task.WhenAll(tcs.Task, pusherTask);
            this.Log().Trace($"cn {cnId} - cleanup connection-bound stores - start");
            await Task.WhenAll(_connectionBoundStores.Select(x => x.Cleanup(_cn.Id)));
            this.Log().Trace($"cn {cnId} - cleanup connection-bound stores - done");
        }
        catch (Exception e)
        {
            this.Log().Error(e);
        }
        finally
        {
            // all handlers must be complete before teardown lifecycle hook
            this.Log().Trace($"cn {cnId} - dispose executor - start");
            await executor.DisposeAsync();
            this.Log().Trace($"cn {cnId} - dispose executor - done");

            // execute end hook
            this.Log().Trace($"cn {cnId} - handle lifecycle end - start");
            await lifeCycleCoordinator.EndAsync(_state);
            this.Log().Trace($"cn {cnId} - handle lifecycle end - done");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _state.DisposeAsync();
    }
}