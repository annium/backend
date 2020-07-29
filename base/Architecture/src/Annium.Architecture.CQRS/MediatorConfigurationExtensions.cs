using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.Core.Runtime.Types;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddCommandQueryHandlers(
            this MediatorConfiguration cfg,
            ITypeManager typeManager
        )
        {
            foreach (var handler in typeManager.GetImplementations(typeof(ICommandHandler<,>)))
                cfg.Add(handler);

            foreach (var handler in typeManager.GetImplementations(typeof(ICommandHandler<>)))
                cfg.Add(handler);

            foreach (var handler in typeManager.GetImplementations(typeof(IQueryHandler<,>)))
                cfg.Add(handler);

            return cfg;
        }
    }
}