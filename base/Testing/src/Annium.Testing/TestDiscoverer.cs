using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Logging.Abstractions;
using Annium.Testing.Elements;

namespace Annium.Testing
{
    public class TestDiscoverer
    {
        private readonly ILogger<TestDiscoverer> _logger;

        public TestDiscoverer(ILogger<TestDiscoverer> logger)
        {
            _logger = logger;
        }

        public Task FindTestsAsync(
            Assembly assembly,
            Action<Test> handleFound
        )
        {
            assembly.GetTypes()
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .ForAll(type => FindTestClassTests(type, handleFound));

            return Task.CompletedTask;
        }

        private void FindTestClassTests(Type testClass, Action<Test> handleTestFound)
        {
            if (testClass.GetCustomAttribute<SkipAttribute>() != null)
            {
                _logger.Debug($"{nameof(FindTestClassTests)}: {testClass.FullName} is skipped");
                return;
            }

            _logger.Trace($"{nameof(FindTestClassTests)} in {testClass.FullName}");
            foreach (var test in testClass.GetMethods().Where(IsTest).Select(method => new Test(method)))
                handleTestFound(test);
        }

        private bool IsTest(MethodInfo candidate) =>
            candidate.GetCustomAttribute<FactAttribute>() != null &&
            candidate.GetCustomAttribute<SkipAttribute>() == null &&
            !candidate.IsGenericMethod &&
            candidate.GetParameters().Length == 0;
    }
}