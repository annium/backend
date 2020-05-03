using System;
using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddCommandQueryHandlers(this MediatorConfiguration cfg)
        {
            foreach (var handler in GetImplementations(typeof(ICommandHandler<,>)))
                cfg.Add(handler);

            foreach (var handler in GetImplementations(typeof(ICommandHandler<>)))
                cfg.Add(handler);

            foreach (var handler in GetImplementations(typeof(IQueryHandler<,>)))
                cfg.Add(handler);

            return cfg;

        }

        private static Type[] GetImplementations(Type type) => TypeManager.Instance.GetImplementations(type);
    }
}