using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing.Elements;
using Annium.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace Annium.Testing.TestAdapter
{
    [ExtensionUri(Constants.ExecutorUri)]
    public class AdapterTestExecutor : ITestExecutor
    {
        private readonly TestConverter testConverter;

        private readonly TestResultConverter testResultConverter;

        private IServiceProvider provider;

        private ILogger logger;

        public AdapterTestExecutor()
        {
            var factory = new ServiceProviderFactory();
            var provider = factory.CreateServiceProvider(factory.CreateBuilder(new ServiceCollection()).UseServicePack<ServicePack>());
            testConverter = provider.GetRequiredService<TestConverter>();
            testResultConverter = provider.GetRequiredService<TestResultConverter>();
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (runContext.IsBeingDebugged)
                Debugger.Launch();

            provider = AdapterServiceProviderBuilder.Build(runContext);
            logger = provider.GetRequiredService<ILogger>();

            logger.LogDebug("Start execution.");

            Task.WhenAll(sources.Select(source => RunAssemblyTestsAsync(Source.Resolve(source), frameworkHandle))).Wait();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            if (runContext.IsBeingDebugged)
                Debugger.Launch();

            provider = AdapterServiceProviderBuilder.Build(runContext);
            logger = provider.GetRequiredService<ILogger>();

            logger.LogDebug("Start execution.");

            Task.WhenAll(tests.Select(test => test.Source).Distinct().Select(
                source => RunAssemblyTestsAsync(Source.Resolve(source), tests.Where(test => test.Source == source), frameworkHandle)
            ));
        }

        public void Cancel() { }

        private async Task RunAssemblyTestsAsync(Assembly assembly, IFrameworkHandle frameworkHandle)
        {
            logger.LogDebug($"Start execution of all tests in {assembly.FullName}.");

            var tests = new List<Test>();
            await provider.GetRequiredService<TestDiscoverer>().FindTestsAsync(assembly, tests.Add);

            await RunTestsAsync(assembly, tests, frameworkHandle);
        }

        private Task RunAssemblyTestsAsync(Assembly assembly, IEnumerable<TestCase> testCases, IFrameworkHandle frameworkHandle)
        {
            logger.LogDebug($"Start execution of specific {testCases.Count()} tests in {assembly.FullName}.");

            var tests = testCases.Select(testCase => testConverter.Convert(assembly, testCase)).ToArray();

            return RunTestsAsync(assembly, tests, frameworkHandle);
        }

        private Task RunTestsAsync(Assembly assembly, IEnumerable<Test> tests, IFrameworkHandle frameworkHandle)
        {
            var cfg = provider.GetRequiredService<TestingConfiguration>();
            tests = tests.FilterMask(cfg.Filter);

            return GetExecutor(assembly, tests)
                .RunTestsAsync(
                    tests,
                    (test, result) => frameworkHandle.RecordResult(testResultConverter.Convert(assembly, test, result))
                );
        }

        private TestExecutor GetExecutor(Assembly assembly, IEnumerable<Test> tests)
        {
            logger.LogTrace($"Build test executor for assembly {assembly.FullName} and given {tests.Count()} tests.");

            var services = AssemblyServicesCollector.Collect(assembly, tests);
            services.AddSingleton(this.provider.GetRequiredService<TestingConfiguration>());

            var factory = new ServiceProviderFactory();
            var provider = factory.CreateServiceProvider(factory.CreateBuilder(services).UseServicePack<Testing.ServicePack>());

            return provider.GetRequiredService<TestExecutor>();
        }
    }
}