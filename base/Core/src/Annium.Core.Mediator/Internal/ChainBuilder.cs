using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Application.Types;
using Annium.Logging.Abstractions;

namespace Annium.Core.Mediator.Internal
{
    internal class ChainBuilder
    {
        private readonly MediatorConfiguration configuration;
        private readonly NextBuilder nextBuilder;
        private readonly ILogger<ChainBuilder> logger;

        public ChainBuilder(
            MediatorConfiguration configuration,
            NextBuilder nextBuilder,
            ILogger<ChainBuilder> logger
        )
        {
            this.configuration = configuration;
            this.nextBuilder = nextBuilder;
            this.logger = logger;
        }

        public IReadOnlyList<ChainElement> BuildExecutionChain(Type input, Type output)
        {
            var handlers = configuration.Handlers.ToList();

            logger.Trace($"Build execution chain for {input} -> {output} from {handlers.Count} handler(s) available");

            var chain = new List<ChainElement>();
            var isFinalized = false;

            while (true)
            {
                logger.Trace($"Find chain element for {input} -> {output}");

                Type service = null;

                foreach (var handler in handlers.ToArray())
                {
                    service = resolveHandler(handler);

                    logger.Trace($"Resolved {handler.RequestIn} -> {handler.ResponseOut} handler into {service}");

                    if (service is null)
                        continue;

                    handlers.Remove(handler);
                    break;
                }

                if (service is null)
                {
                    logger.Trace($"No handler resolved for {input} -> {output}");
                    break;
                }

                logger.Trace($"Add {service} to chain");

                var serviceOutput = service.GetTargetImplementation(Constants.HandlerOutputType);
                // if final handler - break
                if (serviceOutput is null)
                {
                    chain.Add(new ChainElement(service));
                    isFinalized = true;
                    logger.Trace("Resolved handler is final");
                    break;
                }

                var outputArgs = serviceOutput.GetGenericArguments();
                input = outputArgs[0];
                output = outputArgs[1];
                chain.Add(new ChainElement(service, nextBuilder.BuildNextPrototype(input, output)));
            }

            if (!isFinalized)
                throw new InvalidOperationException($"Can't resolve request handler by input {input} and output {output}");

            logger.Trace($"Chain built with {chain.Count} handler(s):");
            foreach (var element in chain)
                logger.Trace($"- {element.Handler}");

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