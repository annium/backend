using Annium.Architecture.CQRS.Commands;
using Annium.Architecture.CQRS.Queries;
using Annium.Core.Application;

namespace Annium.Core.Mediator
{
    public static class MediatorConfigurationExtensions
    {
        public static MediatorConfiguration AddCommandQueryHandlers(this MediatorConfiguration cfg)
        {
            var commandHandlers = TypeManager.Instance.GetImplementations(typeof(ICommandHandler<,>));
            foreach (var handler in commandHandlers)
                cfg.Add(handler);

            var queryHandlers = TypeManager.Instance.GetImplementations(typeof(IQueryHandler<,>));
            foreach (var handler in queryHandlers)
                cfg.Add(handler);

            return cfg;
        }
    }
}