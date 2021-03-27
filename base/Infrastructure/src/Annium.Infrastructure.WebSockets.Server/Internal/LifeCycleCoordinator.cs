using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Infrastructure.WebSockets.Domain.Models;
using Annium.Infrastructure.WebSockets.Server.Handlers;

namespace Annium.Infrastructure.WebSockets.Server.Internal
{
    internal class LifeCycleCoordinator<TState>
        where TState : ConnectionStateBase
    {
        private readonly IServiceProvider _sp;

        public LifeCycleCoordinator(
            IServiceProvider sp
        )
        {
            _sp = sp;
        }

        public async Task HandleStartAsync(TState state)
        {
            using var scope = _sp.CreateScope();
            var handlers = scope.ServiceProvider.Resolve<IEnumerable<ILifeCycleHandler<TState>>>();

            foreach (var handler in handlers)
                await handler.HandleStartAsync(state);
        }

        public async Task HandleEndAsync(TState state)
        {
            using var scope = _sp.CreateScope();
            var handlers = scope.ServiceProvider.Resolve<IEnumerable<ILifeCycleHandler<TState>>>();

            foreach (var handler in handlers)
                await handler.HandleEndAsync(state);
        }
    }
}