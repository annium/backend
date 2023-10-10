using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.Base;
using Annium.Data.Operations;
using Annium.Extensions.Arguments;
using Annium.Logging;
using Annium.Mesh.Client;
using Annium.Mesh.Domain.Requests;
using Demo.Mesh.Domain.Requests.Orders;

namespace Demo.Mesh.Client.Commands;

internal class RequestResponseCommand : AsyncCommand, ICommandDescriptor, ILogSubject
{
    public static string Id => "request-response";
    public static string Description => $"test {Id} flow";
    public ILogger Logger { get; }
    private readonly IClientFactory _clientFactory;

    public RequestResponseCommand(
        IClientFactory clientFactory,
        ILogger logger
    )
    {
        _clientFactory = clientFactory;
        Logger = logger;
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var configuration = new ClientConfiguration()
            .WithResponseTimeout(600);
        var client = _clientFactory.Create(configuration);
        client.OnDisconnected += status =>
        {
            //
            this.Debug("disconnected: {status}", status);
        };
        client.OnConnected += () =>
        {
            //
            this.Debug("connected");
        };

        await client.ConnectAsync();

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
                    // ReSharper disable once AccessToModifiedClosure
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

        this.Debug("End: {elapsed}. Counter: {counter}", sw.Elapsed, counter);

        client.Disconnect();

        async Task<IStatusResult<OperationStatus, T>> Fetch<T>(RequestBase request, CancellationToken token)
        {
            // this.Debug(">>> {request}", request);
            var result = await client.FetchAsync<T>(request, token);
            // this.Debug("<<< {result}", result);
            return result;
        }
    }
}