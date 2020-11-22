using System;
using Annium.Core.DependencyInjection;
using Annium.Testing.Executors;
using NodaTime;

namespace Annium.Testing
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceContainer container, IServiceProvider provider)
        {
            container.Add<Func<Instant>>(SystemClock.Instance.GetCurrentInstant).Singleton();

            // components
            container.Add<TestDiscoverer>().Singleton();
            container.Add<TestExecutor>().Singleton();

            // executors
            container.Add<PipelineExecutor>().Singleton();
            container.Add<ITestExecutor, SkippedExecutor>().Singleton();
            container.Add<ITestExecutor, SetupExecutor>().Singleton();
            container.Add<ITestExecutor, BeforeExecutor>().Singleton();
            container.Add<ITestExecutor, BodyExecutor>().Singleton();
            container.Add<ITestExecutor, AfterExecutor>().Singleton();
            container.Add<MethodExecutor>().Singleton();

            // tools
            container.AddLogging(route =>
            {
                var cfg = provider.Resolve<TestingConfiguration>();
                if (cfg is null)
                    route.UseConsole();
                else
                    route.For(m => m.Level >= cfg.LogLevel).UseConsole();
            });
        }
    }
}