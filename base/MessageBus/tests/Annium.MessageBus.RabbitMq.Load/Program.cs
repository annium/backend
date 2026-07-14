using System;
using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime;
using Annium.Logging.InMemory;
using Annium.Logging.Shared;
using Annium.MessageBus.Abstractions;
using Annium.MessageBus.RabbitMq.Load;
using Annium.MessageBus.Tests.Load.Shared;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;

await using ILoadTransport transport = new RabbitMqLoadTransport();
await transport.StartAsync();

var container = new ServiceContainer();
container.AddRuntime(Assembly.GetExecutingAssembly());
container.AddTime().WithRealTime().SetDefault();
container.AddLogging();
container.AddSerializers().WithJson(isDefault: true);
transport.Configure(container);

var provider = container.BuildServiceProvider();
provider.UseLogging(route => route.UseInMemory());

var harness = new LoadHarness(
    provider.Resolve<IMessagePublisher>(),
    provider.Resolve<IMessageSubscriber>(),
    transport.BrokerName
);

var options = LoadScenarioOptions.Parse(args);
if (transport.MaxPublisherConcurrency > 0)
    options = options with { MaxPublisherConcurrency = transport.MaxPublisherConcurrency };

try
{
    var report = await harness.RunAsync(options);
    LoadReportPrinter.Print(report);
    return report.Passed ? 0 : 1;
}
catch (Exception e)
{
    Console.Error.WriteLine($"PUBLISH FAILED: {e}");
    return 2;
}
