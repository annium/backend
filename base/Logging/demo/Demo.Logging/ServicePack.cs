using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Logging.Shared;

namespace Demo.Logging
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddTimeProvider();

            container.AddLogging(route => route
                .For(m => m.Level == LogLevel.Debug).UseConsole()
                .For(m => m.Level == LogLevel.Trace).UseInMemory());
        }
    }
}