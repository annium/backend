using BenchmarkDotNet.Attributes;

namespace Annium.Net.WebSockets.Benchmark;

[MemoryDiagnoser]
public class ClientServerBenchmark
{
    [Benchmark]
    public void RawAggTrades()
    {
        var c = 0;
        for (var i = 0; i < 100_000_000; i++)
            c++;
    }
}