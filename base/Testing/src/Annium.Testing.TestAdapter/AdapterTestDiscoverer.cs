using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
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
        private readonly TestConverter _testConverter;

        private TestDiscoverer? _testDiscoverer;

        private ILogger<AdapterTestDiscoverer>? _logger;

        public AdapterTestDiscoverer()
        {
            var factory = new ServiceProviderFactory();
            var provider = factory.CreateServiceProvider(factory.CreateBuilder(new ServiceCollection()).UseServicePack<ServicePack>());

            _testConverter = provider.GetRequiredService<TestConverter>();
        }

        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink
        )
        {
            var provider = AdapterServiceProviderBuilder.Build(discoveryContext);
            _testDiscoverer = provider.GetRequiredService<TestDiscoverer>();
            _logger = provider.GetRequiredService<ILogger<AdapterTestDiscoverer>>();

            _logger.Debug("Start discovery.");

            DiscoverSourcesAsync(sources, discoverySink).Wait();
        }

        private Task DiscoverSourcesAsync(IEnumerable<string> sources, ITestCaseDiscoverySink discoverySink) =>
            Task.WhenAll(sources.Select(source => DiscoverAssemblyTestsAsync(source, discoverySink)));

        private Task DiscoverAssemblyTestsAsync(string source, ITestCaseDiscoverySink discoverySink)
        {
            var assembly = Source.Resolve(source);

            _logger!.Debug($"Start discovery of {assembly.FullName}.");

            return _testDiscoverer!.FindTestsAsync(
                assembly,
                test => discoverySink.SendTestCase(_testConverter.Convert(assembly, test))
            );
        }
    }
}