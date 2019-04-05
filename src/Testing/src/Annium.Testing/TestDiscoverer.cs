using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Testing.Elements;
using Annium.Testing.Logging;

namespace Annium.Testing
{
    public class TestDiscoverer
    {
        private readonly ILogger logger;

        public TestDiscoverer(ILogger logger)
        {
            this.logger = logger;
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
            logger.LogDebug($"{nameof(FindTestClassTests)} in {testClass.FullName}");
            foreach (var test in testClass.GetMethods().Where(IsTest).Select(method => new Test(method)))
                handleTestFound(test);
        }

        private bool IsTest(MethodInfo candidate) =>
            candidate.GetCustomAttribute<FactAttribute>() != null &&
            !candidate.IsGenericMethod &&
            candidate.GetParameters().Length == 0;
    }
}