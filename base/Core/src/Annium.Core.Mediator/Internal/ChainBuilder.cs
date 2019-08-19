using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Application.Types;

namespace Annium.Core.Mediator.Internal
{
    internal class ChainBuilder
    {
        private readonly MediatorConfiguration configuration;
        private readonly NextBuilder nextBuilder;

        public ChainBuilder(
            MediatorConfiguration configuration,
            NextBuilder nextBuilder
        )
        {
            this.configuration = configuration;
            this.nextBuilder = nextBuilder;
        }

        public IReadOnlyList<ChainElement> BuildExecutionChain(Type input, Type output)
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
                chain.Add(new ChainElement(service, nextBuilder.BuildNextPrototype(input, output)));
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