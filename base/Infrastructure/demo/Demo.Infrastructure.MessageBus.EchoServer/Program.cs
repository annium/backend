using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Primitives.Threading;
using Annium.Core.Runtime.Time;
using Annium.Infrastructure.MessageBus.Node;
using Annium.Logging.Abstractions;

namespace Demo.Infrastructure.MessageBus.EchoServer
{
    internal class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var logger = provider.Resolve<ILogger<Program>>();
            var socket = provider.Resolve<IMessageBusSocket>();
            var timeProvider = provider.Resolve<ITimeProvider>();

            var cfg = provider.Resolve<EndpointsConfiguration>();
            Console.WriteLine($"Start echo server with PUB {cfg.PubEndpoint} / SUB {cfg.SubEndpoint}");

            socket.Listen(string.Empty).Subscribe(x => logger.Info($"<<<{x}"));

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(500);
                var msg = timeProvider.Now.ToString(null, null);
                Console.WriteLine($">>>{msg}");
                await socket.Send(string.Empty, msg);
            }

            await token;
        }

        internal static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}