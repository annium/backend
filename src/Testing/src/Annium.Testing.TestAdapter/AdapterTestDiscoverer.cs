using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Testing.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace Annium.Testing.TestAdapter
{
    [FileExtension(Constants.FileExtensionDll)]
    [FileExtension(Constants.FileExtensionExe)]
    [DefaultExecutorUri(Constants.ExecutorUri)]
    public class AdapterTestDiscoverer : ITestDiscoverer
    {
        private readonly TestConverter testConverter;

        private TestDiscoverer testDiscoverer;

        private ILogger logger;

        public AdapterTestDiscoverer()
        {
            var provider = new ServiceProviderBuilder().UseServicePack<ServicePack>().Build();
            testConverter = provider.GetRequiredService<TestConverter>();
        }

        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink
        )
        {
            var provider = AdapterServiceProviderBuilder.Build(discoveryContext);
            testDiscoverer = provider.GetRequiredService<TestDiscoverer>();
            this.logger = provider.GetRequiredService<ILogger>();

            this.logger.LogDebug("Start discovery.");

            DiscoverSourcesAsync(sources, discoverySink).Wait();
        }

        private Task DiscoverSourcesAsync(IEnumerable<string> sources, ITestCaseDiscoverySink discoverySink) =>
            Task.WhenAll(sources.Select(source => DiscoverAssemblyTestsAsync(source, discoverySink)));

        private Task DiscoverAssemblyTestsAsync(string source, ITestCaseDiscoverySink discoverySink)
        {
            var assembly = Source.Resolve(source);

            this.logger.LogDebug($"Start discovery of {assembly.FullName}.");

            return testDiscoverer.FindTestsAsync(
                assembly,
                test => discoverySink.SendTestCase(testConverter.Convert(assembly, test))
            );
        }
    }
}