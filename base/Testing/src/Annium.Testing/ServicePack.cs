using System;
using Annium.Core.DependencyInjection;
using Annium.Testing.Executors;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Annium.Testing
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant);

            // components
            services.AddSingleton<TestDiscoverer>();
            services.AddSingleton<TestExecutor>();

            // executors
            services.AddSingleton<PipelineExecutor>();
            services.AddSingleton<ITestExecutor, SkippedExecutor>();
            services.AddSingleton<ITestExecutor, SetupExecutor>();
            services.AddSingleton<ITestExecutor, BeforeExecutor>();
            services.AddSingleton<ITestExecutor, BodyExecutor>();
            services.AddSingleton<ITestExecutor, AfterExecutor>();
            services.AddSingleton<MethodExecutor>();

            // tools
            services.AddLogging(route =>
            {
                var cfg = provider.GetService<TestingConfiguration>();
                if (cfg is null)
                    route.UseConsole();
                else
                    route.For(m => m.Level >= cfg.LogLevel).UseConsole();
            });
        }
    }
}