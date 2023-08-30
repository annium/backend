using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Infrastructure.WebSockets.Domain.Responses;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Internal.Serialization;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging;
using Annium.Net.WebSockets;

namespace Annium.Infrastructure.WebSockets.Server.Internal;

internal class ConnectionHandler<TState> : IAsyncDisposable, ILogSubject
    where TState : ConnectionStateBase
{
    public ILogger Logger { get; }
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
        ILogger logger
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
        this.Trace("cn {connectionId} - start", cnId);
        await using var scope = _sp.CreateAsyncScope();
        var executor = Executor.Background.Parallel<ConnectionHandler<TState>>(Logger);
        var lifeCycleCoordinator = scope.ServiceProvider.Resolve<LifeCycleCoordinator<TState>>();
        var pusherCoordinator = scope.ServiceProvider.Resolve<PusherCoordinator<TState>>();
        try
        {
            var tcs = new TaskCompletionSource();
            tcs.Task.ContinueWith(_ => this.Trace("cn {connectionId} - listen ended"), CancellationToken.None).GetAwaiter();

            // immediately subscribe to cancellation
            ct.Register(() =>
            {
                this.Trace("cn {connectionId} - complete tcs due to cancellation", cnId);
                tcs.TrySetResult();
            });

            // use derived cts for socket subscription
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            // start listening to messages and adding them to scheduler
            this.Trace("cn {connectionId} - init subscription", cnId);
            _cn.Socket
                .ObserveBinary()
                .Subscribe(HandleMessage,
                    e =>
                    {
                        this.Trace("cn {connectionId} - handle error", cnId);
                        cts.Cancel();
                        this.Error(e);
                        this.Trace("cn {connectionId} - complete tcs due to error", cnId);
                        tcs.TrySetResult();
                    },
                    () =>
                    {
                        this.Trace("cn {connectionId} - complete tcs due to socket closed", cnId);
                        tcs.TrySetResult();
                    },
                    cts.Token
                );

            // execute start hook
            this.Trace("cn {connectionId} - handle lifecycle start - start", cnId);
            await lifeCycleCoordinator.StartAsync(_state);
            this.Trace("cn {connectionId} - handle lifecycle start - done", cnId);

            // notify client, that connection is ready
            this.Trace("cn {connectionId} - notify connection ready", cnId);
            await _cn.Socket.SendBinaryAsync(_serializer.Serialize(new ConnectionReadyNotification()), cts.Token);

            // execute run hook
            this.Trace("cn {connectionId} - push handlers start - start", cnId);
            var pusherTask = pusherCoordinator.RunAsync(_state, cts.Token);
            pusherTask.ContinueWith(_ => this.Trace("cn {connectionId} - push ended"), CancellationToken.None).GetAwaiter();
            this.Trace("cn {connectionId} - push handlers start - done", cnId);

            // start scheduler to process backlog and run upcoming work immediately
            this.Trace("cn {connectionId} - start executor", cnId);
            executor.Start(CancellationToken.None);

            // wait until connection complete
            this.Trace("cn {connectionId} - wait until connection complete", cnId);
            await Task.WhenAll(tcs.Task, pusherTask);
            this.Trace("cn {connectionId} - cleanup connection-bound stores - start", cnId);
            await Task.WhenAll(_connectionBoundStores.Select(x => x.Cleanup(_cn.Id)));
            this.Trace("cn {connectionId} - cleanup connection-bound stores - done", cnId);
        }
        catch (Exception e)
        {
            this.Error(e);
        }
        finally
        {
            // all handlers must be complete before teardown lifecycle hook
            this.Trace("cn {connectionId} - dispose executor - start", cnId);
            await executor.DisposeAsync();
            this.Trace("cn {connectionId} - dispose executor - done", cnId);

            // execute end hook
            this.Trace("cn {connectionId} - handle lifecycle end - start", cnId);
            await lifeCycleCoordinator.EndAsync(_state);
            this.Trace("cn {connectionId} - handle lifecycle end - done", cnId);
        }

        void HandleMessage(ReadOnlyMemory<byte> raw)
        {
            var request = ParseRequest(raw);
            if (request is null)
            {
                this.Warn("Failed to parse msg of size {size} bytes", raw.Length);

                return;
            }

            this.Trace("cn {connectionId} - schedule {requestType}#{requestId}", cnId, request.Tid, request.Rid);
            executor.TrySchedule(async () =>
            {
                this.Trace("cn {connectionId} - handle {requestType}#{requestId}", cnId, request.Tid, request.Rid);
                await using var messageScope = _sp.CreateAsyncScope();
                var handler = messageScope.ServiceProvider.Resolve<MessageHandler<TState>>();
                await handler.HandleMessage(_cn.Socket, _state, request);
            });
        }
    }

    private AbstractRequestBase? ParseRequest(ReadOnlyMemory<byte> msg)
    {
        try
        {
            return _serializer.Deserialize<AbstractRequestBase>(msg);
        }
        catch (Exception e)
        {
            this.Warn(e.ToString());
            return default;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _state.DisposeAsync();
    }
}