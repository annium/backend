using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Infrastructure.WebSockets.Server.Models;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class LifeCycleCoordinator<TState> : ILogSubject
        where TState : ConnectionStateBase
    {
        public ILogger Logger { get; }
        private readonly IEnumerable<ILifeCycleHandler<TState>> _handlers;

        public LifeCycleCoordinator(
            IEnumerable<ILifeCycleHandler<TState>> handlers,
            ILogger<LifeCycleCoordinator<TState>> logger
        )
        {
            _handlers = handlers;
            Logger = logger;
        }

        public Task HandleStartAsync(TState state)
        {
            this.Log().Trace("start");

            return HandleAsync(state, (x, s) => x.HandleStartAsync(s));
        }

        public Task HandleEndAsync(TState state)
        {
            this.Log().Trace("start");

            return HandleAsync(state, (x, s) => x.HandleEndAsync(s));
        }

        private async Task HandleAsync(TState state, Func<ILifeCycleHandler<TState>, TState, Task> handleState)
        {
            foreach (var handler in _handlers.OrderBy(x => x.Order))
                await handleState(handler, state);
        }
    }
}