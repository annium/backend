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

        public Task HandleStartAsync(TState state) => HandleAsync(state, (x, s) => x.HandleStartAsync(s));

        public Task HandleEndAsync(TState state) => HandleAsync(state, (x, s) => x.HandleEndAsync(s));

        private async Task HandleAsync(TState state, Func<ILifeCycleHandler<TState>, TState, Task> handleState)
        {
            var scope = _sp.CreateScope();
            try
            {
                var handlers = scope.ServiceProvider.Resolve<IEnumerable<ILifeCycleHandler<TState>>>();

                foreach (var handler in handlers)
                    await handleState(handler, state);
            }
            finally
            {
                if (scope is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else if (scope is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }
}