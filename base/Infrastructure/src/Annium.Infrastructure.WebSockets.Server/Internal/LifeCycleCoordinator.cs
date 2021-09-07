using System.Collections.Generic;
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
        private readonly IEnumerable<LifeCycleHandlerBase<TState>> _handlers;

        public LifeCycleCoordinator(
            IEnumerable<LifeCycleHandlerBase<TState>> handlers,
            ILogger<LifeCycleCoordinator<TState>> logger
        )
        {
            _handlers = handlers;
            Logger = logger;
        }

        public async Task StartAsync(TState state)
        {
            this.Log().Trace("start");

            foreach (var handler in _handlers)
                await handler.StartAsync(state);
        }

        public async Task EndAsync(TState state)
        {
            this.Log().Trace("start");

            foreach (var handler in _handlers)
                await handler.EndAsync(state);
        }
    }
}