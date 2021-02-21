using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Primitives;
using Annium.Core.Reflection;
using Annium.Logging.Abstractions;

namespace Annium.Core.Mediator.Internal
{
    internal class ChainBuilder
    {
        private readonly MediatorConfiguration _configuration;
        private readonly NextBuilder _nextBuilder;
        private readonly ILogger<ChainBuilder> _logger;

        public ChainBuilder(
            IEnumerable<MediatorConfiguration> configurations,
            NextBuilder nextBuilder,
            ILogger<ChainBuilder> logger
        )
        {
            _configuration = MediatorConfiguration.Merge(configurations.ToArray());
            _nextBuilder = nextBuilder;
            _logger = logger;
        }

        public IReadOnlyList<ChainElement> BuildExecutionChain(Type input, Type output)
        {
            var handlers = _configuration.Handlers.ToList();

            output = ResolveOutput(input, output);

            _logger.Trace($"Build execution chain for {input.FriendlyName()} -> {output.FriendlyName()} from {handlers.Count} handler(s) available");

            var chain = new List<ChainElement>();
            var isFinalized = false;

            while (true)
            {
                _logger.Trace($"Find chain element for {input.FriendlyName()} -> {output.FriendlyName()}");

                Type? service = null;

                foreach (var handler in handlers.ToArray())
                {
                    service = ResolveHandler(input, output, handler);

                    _logger.Trace($"Resolved {handler.RequestIn.FriendlyName()} -> {handler.ResponseOut.FriendlyName()} handler into {service?.FriendlyName() ?? null}");

                    if (service is null)
                        continue;

                    handlers.Remove(handler);
                    break;
                }

                if (service is null)
                {
                    _logger.Trace($"No handler resolved for {input.FriendlyName()} -> {output.FriendlyName()}");
                    break;
                }

                _logger.Trace($"Add {service.FriendlyName()} to chain");

                var serviceOutput = service.GetTargetImplementation(Constants.HandlerOutputType);
                // if final handler - break
                if (serviceOutput is null)
                {
                    chain.Add(new ChainElement(service));
                    isFinalized = true;
                    _logger.Trace("Resolved handler is final");
                    break;
                }

                var outputArgs = serviceOutput.GetGenericArguments();
                input = outputArgs[0];
                output = outputArgs[1];
                chain.Add(new ChainElement(service, _nextBuilder.BuildNext(input, output)));
            }

            TraceChain(chain);

            if (!isFinalized)
                throw new InvalidOperationException($"Can't resolve request handler by input {input.FriendlyName()} and output {output.FriendlyName()}");

            return chain;
        }

        private Type ResolveOutput(Type input, Type output)
        {
            var match = _configuration.Matches
                .SingleOrDefault(x => x.RequestedType == input && x.ExpectedType == output);

            return match?.ResolvedType ?? output;
        }

        private Type? ResolveHandler(Type input, Type output, Handler handler)
        {
            var requestIn = input.GetTargetImplementation(handler.RequestIn);
            // var responseOut = handler.ResponseOut.ResolveByImplentations(output);
            var responseOut = output.GetTargetImplementation(handler.ResponseOut);

            if (requestIn is null || responseOut is null)
                return null;

            var handlerInput = Constants.HandlerInputType.MakeGenericType(requestIn, responseOut);
            var service = handler.Implementation.ResolveByImplementation(handlerInput);
            if (service is null)
            {
                _logger.Trace($"Can't resolve {handler.Implementation.FriendlyName()} by input {requestIn.FriendlyName()} and output {responseOut.FriendlyName()}");
                return null;
            }

            return service;
        }

        private void TraceChain(IReadOnlyCollection<ChainElement> chain)
        {
            _logger.Trace($"Composed chain with {chain.Count} handler(s):");
            foreach (var element in chain)
                _logger.Trace($"- {element.Handler}");
        }
    }
}