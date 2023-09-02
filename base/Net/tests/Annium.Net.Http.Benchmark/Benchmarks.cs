using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Net.Http.Benchmark.Internal;
using BenchmarkDotNet.Attributes;
using NativeClientWebSocket = System.Net.WebSockets.ClientWebSocket;

namespace Annium.Net.Http.Benchmark;

[MemoryDiagnoser]
public class Benchmarks
{
    [Params(1000, 10_000)]
    public int TotalRequests { get; set; }

    private static readonly Uri ServerUri = new($"http://127.0.0.1:{Constants.Port}/");
    private readonly IHttpRequestFactory _httpRequestFactory;

    public Benchmarks()
    {
        var container = new ServiceContainer();
        container.AddHttpRequestFactory(true);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging();

        var sp = container.BuildServiceProvider();
        sp.UseLogging(route => route.UseInMemory());

        _httpRequestFactory = sp.Resolve<IHttpRequestFactory>();
    }

    [Benchmark]
    public async Task Plain()
    {
        for (var i = 0; i < TotalRequests; i++)
        {
            var response = await _httpRequestFactory.New(ServerUri)
                .Get("/")
                .RunAsync();
            if (response.IsFailure)
                throw new Exception($"Response #{i} failed");
        }
    }
}