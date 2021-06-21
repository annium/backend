using System;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;

namespace Demo.Logging
{
    internal class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddTimeProvider();

            container.AddLogging(route => route
                // .For(m => m.Level == LogLevel.Info).UseSeq("http://localhost:9905")
                .For(m => m.Level == LogLevel.Debug).UseConsole()
                .For(m => m.Level == LogLevel.Trace).UseInMemory());
            container.AddMapper();
            container.AddArguments();

            // commands
            container.AddAll(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .Singleton();
        }
    }
}