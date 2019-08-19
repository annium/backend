using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Application.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.Mediator.Internal
{
    internal class Mediator : IMediator
    {
        private readonly MediatorConfiguration configuration;
        private readonly IServiceProvider provider;
        private readonly NextBuilder nextBuilder;

        public Mediator(
            MediatorConfiguration configuration,
            IServiceProvider provider,
            NextBuilder nextBuilder
        )
        {
            this.configuration = configuration;
            this.provider = provider;
            this.nextBuilder = nextBuilder;
        }

        public async Task<TResponse> SendAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default
        )
        {
            // get execution chain with last item, being final one
            var chain = GetExecutionChain(typeof(TRequest), typeof(TResponse));

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

        private IReadOnlyList<ChainElement> GetExecutionChain(Type input, Type output)
        {
            var handlers = configuration.Handlers.ToList();
            var chain = new List<ChainElement>();
            var isFinalized = false;

            while (true)
            {
                Type service = null;

                foreach (var handler in handlers.ToArray())
                {
                    service = resolveHandler(handler);
                    if (service is null)
                        continue;

                    handlers.Remove(handler);
                    break;
                }

                if (service is null)
                    break;

                var serviceOutput = service.GetTargetImplementation(Constants.HandlerOutputType);
                // if final handler - break
                if (serviceOutput is null)
                {
                    chain.Add(new ChainElement(service));
                    isFinalized = true;
                    break;
                }

                var outputArgs = serviceOutput.GetGenericArguments();
                input = outputArgs[0];
                output = outputArgs[1];
                chain.Add(new ChainElement(service, (input, output)));
            }

            if (!isFinalized)
                throw new InvalidOperationException($"Can't resolve request handler by input {input} and output {output}");

            return chain;

            Type resolveHandler(Handler handler)
            {
                var requestIn = input.GetTargetImplementation(handler.RequestIn);
                var responseOut = handler.ResponseOut.ResolveByImplentations(output);
                // var responseOut = output.GetTargetImplementation(handler.ResponseOut);

                if (requestIn is null || responseOut is null)
                    return null;

                var handlerInput = typeof(IRequestHandlerInput<,>).MakeGenericType(requestIn, responseOut);
                var service = handler.Implementation.ResolveByImplentations(handlerInput);
                if (service is null)
                    throw new InvalidOperationException($"Can't resolve {handler.Implementation} by input {requestIn} and output {responseOut}");

                return service;
            }
        }
    }
}