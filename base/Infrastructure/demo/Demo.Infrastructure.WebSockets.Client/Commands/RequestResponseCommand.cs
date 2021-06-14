using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Arguments;
using Annium.Infrastructure.WebSockets.Client;
using Annium.Infrastructure.WebSockets.Domain.Requests;
using Annium.Logging.Abstractions;
using Demo.Infrastructure.WebSockets.Client.Commands.Demo;
using Demo.Infrastructure.WebSockets.Domain.Requests.Orders;

namespace Demo.Infrastructure.WebSockets.Client.Commands
{
    internal class RequestResponseCommand : AsyncCommand<ServerCommandConfiguration>, ILogSubject
    {
        public override string Id { get; } = "request-response";
        public override string Description => $"test {Id} flow";
        public ILogger Logger { get; }
        private readonly IClientFactory _clientFactory;

        public RequestResponseCommand(
            IClientFactory clientFactory,
            ILogger<RequestCommand> logger
        )
        {
            _clientFactory = clientFactory;
            Logger = logger;
        }

        public override async Task HandleAsync(ServerCommandConfiguration cfg, CancellationToken ct)
        {
            var configuration = new ClientConfiguration()
                .ConnectTo(cfg.Server)
                .WithActiveKeepAlive(600)
                .WithResponseTimeout(600);
            var client = _clientFactory.Create(configuration);
            client.ConnectionLost += () =>
            {
                this.Debug("connection lost");
                return Task.CompletedTask;
            };
            client.ConnectionRestored += () =>
            {
                this.Debug("connection restored");
                return Task.CompletedTask;
            };

            await client.ConnectAsync(ct);

            var counter = 0;
            var sw = new Stopwatch();

            this.Debug("Parallel");
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

            this.Debug("Sequential");
            sw.Start();
            foreach (var _ in Enumerable.Range(0, 5000))
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var value = await Fetch<int>(new CreateOrderRequest(), cts.Token);
                Interlocked.Add(ref counter, value.Data);
            }

            sw.Stop();

            this.Debug($"End: {sw.Elapsed}. Counter: {counter}");

            if (client.IsConnected)
                await client.DisconnectAsync();

            async Task<IStatusResult<OperationStatus, T>> Fetch<T>(RequestBase request, CancellationToken ct)
            {
                // this.Debug($">>> {request}");
                var result = await client!.FetchAsync<T>(request, ct);
                // this.Debug($"<<< {result}");
                return result;
            }
        }
    }
}