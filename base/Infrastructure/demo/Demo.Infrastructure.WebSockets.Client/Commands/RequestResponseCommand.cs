using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Time;
using Annium.Data.Operations;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Infrastructure.WebSockets.Client.Internal;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Logging.Abstractions;
using Annium.Serialization.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;
using NodaTime;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class RequestResponseCommand : AsyncCommand<ServerCommandConfiguration>
    {
        private readonly IClientFactory _clientFactory;
        private readonly ILogger<RequestCommand> _logger;
        public override string Id { get; } = "request-response";
        public override string Description => $"test {Id} flow";

        public RequestResponseCommand(
            IClientFactory clientFactory,
            ILogger<RequestCommand> logger
        )
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken token)
        {
            var configuration = new ClientConfiguration().ConnectTo(cfg.Server).WithAutoReconnect();
            var client = _clientFactory.Create(configuration);
            client.ConnectionLost += () => _logger.Debug("connection lost");
            client.ConnectionRestored += () => _logger.Debug("connection restored");

            await client.ConnectAsync(token);

            var counter = 0;
            var sw = new Stopwatch();

            _logger.Debug("Parallel");
            sw.Start();
            await Task.WhenAll(
                Enumerable.Range(0, 20000)
                    .Select(async _ =>
                    {
                        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                        var value = await Fetch<int>(new CreateOrderRequest(), cts.Token);
                        Interlocked.Add(ref counter, value.Data);
                    })
            );
            sw.Stop();

            _logger.Debug("Sequential");
            sw.Start();
            foreach (var _ in Enumerable.Range(0, 5000))
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var value = await Fetch<int>(new CreateOrderRequest(), cts.Token);
                Interlocked.Add(ref counter, value.Data);
            }

            sw.Stop();

            _logger.Debug($"End: {sw.Elapsed}. Counter: {counter}");

            if (client.IsConnected)
                await client.DisconnectAsync(CancellationToken.None);

            async Task<IStatusResult<OperationStatus, T>> Fetch<T>(RequestBase request, CancellationToken ct)
            {
                // _logger.Debug($">>> {request}");
                var result = await client!.Fetch<T>(request, ct);
                // _logger.Debug($"<<< {result}");
                return result;
            }
        }
    }
}