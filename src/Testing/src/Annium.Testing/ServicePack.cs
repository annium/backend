using System;
using Annium.Extensions.DependencyInjection;
using Annium.Testing.Executors;
using Annium.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Testing
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
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
            services.AddSingleton<ILogger, Logger>();
        }
    }
}