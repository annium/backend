using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Handlers;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class LifeCycleCoordinator
    {
        private readonly IEnumerable<ILifeCycleHandler> _handlers;

        public LifeCycleCoordinator(
            IEnumerable<ILifeCycleHandler> handlers
        )
        {
            _handlers = handlers;
        }

        public async Task HandleStartAsync(IConnectionState state)
        {
            foreach (var handler in _handlers)
                await handler.HandleStartAsync(state);
        }

        public async Task HandleEndAsync(IConnectionState state)
        {
            foreach (var handler in _handlers)
                await handler.HandleEndAsync(state);
        }
    }
}