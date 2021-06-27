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
            container.AddRuntimeTools(GetType().Assembly, false);
            container.AddTimeProvider();

            container.AddLogging(route => route
                .For(m => m.Level == LogLevel.Info).UseSeq(new Uri("http://localhost:5341"),new LogRouteConfiguration())
                .For(m => m.Level == LogLevel.Debug).UseConsole()
                .For(m => m.Level == LogLevel.Trace).UseInMemory());
            container.AddMapper();
            container.AddArguments();
            container.AddJsonSerializers().SetDefault();
            container.AddHttpRequestFactory().SetDefault();

            // commands
            container.AddAll(GetType().Assembly)
                .Where(x => x.Name.EndsWith("Group") || x.Name.EndsWith("Command"))
                .AsSelf()
                .Singleton();
        }
    }
}