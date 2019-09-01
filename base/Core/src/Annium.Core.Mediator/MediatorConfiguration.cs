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
            var result = new MediatorConfiguration();
            result.handlers = configurations.SelectMany(c => c.Handlers).ToList();

            return result;
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

            var serviceType = interfaces.FirstOrDefault(i => i.GetGenericTypeDefinition() == Constants.PipeHandlerType);
            if (serviceType != null)
            {
                var args = serviceType.GetGenericArguments();
                handlers.Add(new Handler(handlerType, args[0], args[1], args[2], args[3]));
                return this;
            }

            serviceType = interfaces.FirstOrDefault(i => i.GetGenericTypeDefinition() == Constants.FinalHandlerType);
            if (serviceType != null)
            {
                var args = serviceType.GetGenericArguments();;
                handlers.Add(new Handler(handlerType, args[0], null, null, args[1]));
                return this;
            }

            throw new InvalidOperationException(
                $"To register {handlerType.FullName} as Mediator request handler, it must implement {Constants.PipeHandlerType.FullName} or {Constants.FinalHandlerType.FullName}"
            );
        }

        // TODO: allow, if will be needed
        // public void Add<THandler, TRequestIn, TRequestOut, TResponseIn, TResponseOut>()
        // where THandler : IRequestHandler<TRequestIn, TRequestOut, TResponseIn, TResponseOut>
        // {
        //     handlers.Add(new Record(typeof(TRequestIn), typeof(TRequestOut), typeof(TResponseIn), typeof(TResponseOut), typeof(THandler)));
        // }
    }
}