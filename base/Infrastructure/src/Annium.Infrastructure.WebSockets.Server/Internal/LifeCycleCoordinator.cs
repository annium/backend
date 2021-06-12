using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Handlers;
using Annium.Logging.Abstractions;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class LifeCycleCoordinator<TState>
        where TState : ConnectionStateBase
    {
        private readonly IEnumerable<ILifeCycleHandler<TState>> _handlers;
        private readonly ILogger<LifeCycleCoordinator<TState>> _logger;

        public LifeCycleCoordinator(
            IEnumerable<ILifeCycleHandler<TState>> handlers,
            ILogger<LifeCycleCoordinator<TState>> logger
        )
        {
            _handlers = handlers;
            _logger = logger;
        }

        public Task HandleStartAsync(TState state)
        {
            _logger.Trace("start");

            return HandleAsync(state, (x, s) => x.HandleStartAsync(s));
        }

        public Task HandleEndAsync(TState state)
        {
            _logger.Trace("start");

            return HandleAsync(state, (x, s) => x.HandleEndAsync(s));
        }

        private async Task HandleAsync(TState state, Func<ILifeCycleHandler<TState>, TState, Task> handleState)
        {
            foreach (var handler in _handlers.OrderBy(x => x.Order))
                await handleState(handler, state);
        }
    }
}