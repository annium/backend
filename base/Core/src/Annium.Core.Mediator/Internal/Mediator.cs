using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mediator.Internal
{
    internal class Mediator : IMediator
    {
        private readonly ChainBuilder chainBuilder;
        private readonly IServiceProvider provider;
        private readonly IDictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>> chainCache =
            new Dictionary<ValueTuple<Type, Type>, IReadOnlyList<ChainElement>>();

        public Mediator(
            ChainBuilder chainBuilder,
            IServiceProvider provider
        )
        {
            this.chainBuilder = chainBuilder;
            this.provider = provider;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default
        )
        {
            // get execution chain with last item, being final one
            var chain = GetChain(typeof(TRequest), typeof(TResponse));

            // use scoped service provider
            using var scope = provider.CreateScope();

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
            lock(chainCache)
            {
                var key = (input, output);
                if (chainCache.TryGetValue(key, out var chain))
                    return chain;

                return chainCache[key] = chainBuilder.BuildExecutionChain(input, output);
            }
        }
    }
}