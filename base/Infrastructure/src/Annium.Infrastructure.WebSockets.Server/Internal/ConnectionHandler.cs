using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Extensions.Execution;
using Annium.Infrastructure.WebSockets.Server.Internal.Models;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class ConnectionHandler<TState> : IAsyncDisposable, ILogSubject
        where TState : ConnectionStateBase
    {
        public ILogger Logger { get; }
        private readonly IServiceProvider _sp;
        private readonly IEnumerable<IConnectionBoundStore> _connectionBoundStores;
        private readonly Connection _cn;
        private readonly TState _state;

        public ConnectionHandler(
            IServiceProvider sp,
            Func<Guid, TState> stateFactory,
            IEnumerable<IConnectionBoundStore> connectionBoundStores,
            Connection cn,
            ILogger<ConnectionHandler<TState>> logger
        )
        {
            _sp = sp;
            _connectionBoundStores = connectionBoundStores;
            _cn = cn;
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
            try
            {
                var tcs = new TaskCompletionSource<object>();

                // immediately subscribe to cancellation
                ct.Register(() => tcs.TrySetResult(new object()));

                // start listening to messages and adding them to scheduler
                this.Log().Trace($"cn {cnId} - init subscription");
                _cn.Socket
                    .Listen()
                    .Subscribe(
                        x => executor.TrySchedule(async () =>
                        {
                            this.Log().Trace($"cn {cnId} - HandleMessage");
                            await using var messageScope = _sp.CreateAsyncScope();
                            var handler = messageScope.ServiceProvider.Resolve<MessageHandler<TState>>();
                            await handler.HandleMessage(_cn.Socket, _state, x);
                        }),
                        x => tcs.TrySetException(x),
                        () => tcs.TrySetResult(new object()),
                        ct
                    );

                // process start hook
                this.Log().Trace($"cn {cnId} - handle lifecycle start - start");
                await lifeCycleCoordinator.HandleStartAsync(_state);
                this.Log().Trace($"cn {cnId} - handle lifecycle start - done");

                // start scheduler to process backlog and run upcoming work immediately
                this.Log().Trace($"cn {cnId} - start executor");
                executor.Start(CancellationToken.None);

                // wait until connection complete
                this.Log().Trace($"cn {cnId} - wait until connection complete");
                await tcs.Task;
                this.Log().Trace($"cn {cnId} - cleanup connection-bound stores - start");
                await Task.WhenAll(_connectionBoundStores.Select(x => x.Cleanup(_cn.Id)));
                this.Log().Trace($"cn {cnId} - cleanup connection-bound stores - done");
            }
            finally
            {
                // all handlers must be complete before teardown lifecycle hook
                this.Log().Trace($"cn {cnId} - dispose executor - start");
                await executor.DisposeAsync();
                this.Log().Trace($"cn {cnId} - dispose executor - done");

                // process end hook
                this.Log().Trace($"cn {cnId} - handle lifecycle end - start");
                await lifeCycleCoordinator.HandleEndAsync(_state);
                this.Log().Trace($"cn {cnId} - handle lifecycle end - done");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _state.DisposeAsync();
        }
    }
}