using System;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Demo.Core.Mediator.Db;
using Demo.Core.Mediator.Handlers;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Demo.Core.Mediator
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);
            services.AddMediatorConfiguration(ConfigureMediator);
            services.AddMediator();

            services.AddSingleton<TodoRepository>();

            services.AddLogging(route => route.UseConsole());
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