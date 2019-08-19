using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mediator.Internal
{
    internal class Mediator : IMediator
    {
        private readonly ChainBuilder chainBuilder;
        private readonly NextBuilder nextBuilder;
        private readonly MediatorConfiguration configuration;
        private readonly IServiceProvider provider;

        public Mediator(
            ChainBuilder chainBuilder,
            NextBuilder nextBuilder,
            MediatorConfiguration configuration,
            IServiceProvider provider
        )
        {
            this.chainBuilder = chainBuilder;
            this.nextBuilder = nextBuilder;
            this.configuration = configuration;
            this.provider = provider;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default
        )
        {
            // get execution chain with last item, being final one
            var chain = chainBuilder.BuildExecutionChain(typeof(TRequest), typeof(TResponse));

            // execute chain
            return (TResponse) await ExecuteAsync(chain, request, cancellationToken);
        }

        internal async Task<object> ExecuteAsync(
            IReadOnlyList<ChainElement> chain,
            object request,
            CancellationToken cancellationToken,
            int index = 0
        )
        {
            var isFinal = index < chain.Count() - 1;
            var element = chain[index];

            var parameters = new List<object> { request, cancellationToken };
            if (isFinal)
                parameters.Add(nextBuilder.BuildNext(this, element, chain, cancellationToken, index + 1));

            var handler = element.Handler;
            var handleMethodName = isFinal ? Constants.FinalHandlerHandleAsyncName : Constants.PipeHandlerHandleAsyncName;
            var handleMethod = handler.GetMethod(handleMethodName, parameters.Select(p => p.GetType()).ToArray());
            var result = handleMethod.Invoke(provider.GetRequiredService(handler), parameters.ToArray());
            await ((Task) result);

            return result.GetType().GetProperty(Constants.TaskResultName)
                .GetGetMethod().Invoke(result, Array.Empty<object>());
        }
    }
}