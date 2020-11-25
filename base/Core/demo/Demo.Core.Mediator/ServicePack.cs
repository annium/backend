using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Demo.Core.Mediator.Db;
using Demo.Core.Mediator.Handlers;

namespace Demo.Core.Mediator
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();
            container.AddMediatorConfiguration(ConfigureMediator);
            container.AddMediator();

            container.Add<TodoRepository>().Singleton();

            container.AddLogging(route => route.UseConsole());
        }

        private void ConfigureMediator(MediatorConfiguration cfg)
        {
            cfg.Add(typeof(LoggingHandler<,>));
            cfg.Add(typeof(ConversionHandler<,>));
            cfg.Add(typeof(ExceptionHandler<,>));
            cfg.Add(typeof(ValidationHandler<,>));
            cfg.Add(typeof(AuthorizationHandler<,>));
            cfg.Add(typeof(TodoCommandHandler));
        }
    }
}