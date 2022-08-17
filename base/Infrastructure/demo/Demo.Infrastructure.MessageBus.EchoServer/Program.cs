using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Threading;
using Annium.Infrastructure.MessageBus.Node;
using Demo.Infrastructure.MessageBus.EchoServer;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, ct) = entry;

var socket = provider.Resolve<IMessageBusSocket>();
var timeProvider = provider.Resolve<ITimeProvider>();

var cfg = provider.Resolve<EndpointsConfiguration>();
Console.WriteLine($"Start echo server with PUB {cfg.PubEndpoint} / SUB {cfg.SubEndpoint}");

socket.Subscribe(x => Console.WriteLine($"<<<{x}"));

while (!ct.IsCancellationRequested)
{
    await Task.Delay(500);
    var msg = timeProvider.Now.ToString(null, null);
    Console.WriteLine($">>>{msg}");
    await socket.Send(msg);
}

await ct;