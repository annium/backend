using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mediator.Internal
{
    internal class Mediator : IMediator
    {
        private readonly ChainBuilder _chainBuilder;
        private readonly IServiceProvider _provider;

        private readonly IDictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>> _chainCache =
            new Dictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>>();

        public Mediator(
            ChainBuilder chainBuilder,
            IServiceProvider provider
        )
        {
            _chainBuilder = chainBuilder;
            _provider = provider;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default
        )
        {
            // get execution chain with last item, being final one
            var chain = GetChain(typeof(TRequest), typeof(TResponse));

            // use scoped service provider
            using var scope = _provider.CreateScope();

            return (TResponse) await ChainExecutor.ExecuteAsync(scope.ServiceProvider, chain, request!, cancellationToken);
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            IServiceProvider serviceProvider,
            TRequest request,
            CancellationToken cancellationToken = default
        )
        {
            // get execution chain with last item, being final one
            var chain = GetChain(typeof(TRequest), typeof(TResponse));

            // use given service provider
            return (TResponse) await ChainExecutor.ExecuteAsync(serviceProvider, chain, request!, cancellationToken);
        }

        private IReadOnlyList<ChainElement> GetChain(Type input, Type output)
        {
            lock (_chainCache)
            {
                var key = (input, output);
                if (_chainCache.TryGetValue(key, out var chain))
                    return chain;

                return _chainCache[key] = _chainBuilder.BuildExecutionChain(input, output);
            }
        }
    }
}