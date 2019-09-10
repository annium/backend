using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.Mediator.Internal;

namespace Annium.Core.Mediator
{
    public class MediatorConfiguration
    {
        internal static MediatorConfiguration Merge(params MediatorConfiguration[] configurations)
        {
            return new MediatorConfiguration
            {
                handlers = configurations.SelectMany(c => c.Handlers).ToList()
            };
        }

        internal IEnumerable<Handler> Handlers => handlers;
        private IList<Handler> handlers = new List<Handler>();

        internal MediatorConfiguration() { }

        public MediatorConfiguration Add(Type handlerType)
        {
            // ensure type is pipe or final handler
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType)

                .ToArray();

            var isRegistered = false;

            foreach (var serviceType in interfaces.Where(i => i.GetGenericTypeDefinition() == Constants.PipeHandlerType))
            {
                var args = serviceType.GetGenericArguments();
                handlers.Add(new Handler(handlerType, args[0], args[1], args[2], args[3]));
                isRegistered = true;
            }

            foreach (var serviceType in interfaces.Where(i => i.GetGenericTypeDefinition() == Constants.FinalHandlerType))
            {
                var args = serviceType.GetGenericArguments();
                handlers.Add(new Handler(handlerType, args[0], null, null, args[1]));
                isRegistered = true;
            }

            if (isRegistered)
                return this;

            throw new InvalidOperationException(
                $"To register {handlerType.FullName} as Mediator request handler, it must implement {Constants.PipeHandlerType.FullName} or {Constants.FinalHandlerType.FullName}"
            );
        }
    }
}