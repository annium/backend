using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using BenchmarkDotNet.Attributes;
using Microsoft.IO;

namespace Annium.Net.WebSockets.Benchmark;

[MemoryDiagnoser]
public class ClientServerBenchmark
{
    [Params(1_000_000)]
    public long TotalMessages { get; set; }

    private readonly RecyclableMemoryStreamManager _streamManager = new();
    private MemoryStream _ms = default!;
    private ManagedWebSocket _socket = default!;
    private ManualResetEventSlim _gate;
    private long _eventCount;

    [IterationSetup]
    public void IterationSetup()
    {
        _ms = _streamManager.GetStream();
        _socket = new ManagedWebSocket(WebSocket.CreateFromStream(_ms, false, null, Timeout.InfiniteTimeSpan));
        _eventCount = TotalMessages;
        _socket.ListenAsync(CancellationToken.None);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        // client.UnSubscribe("btcusdt").Wait();
    }

    [Benchmark]
    public void ReceiveInMemory()
    {
        var c = 0;
        for (var i = 0; i < 100_000_000; i++)
            c++;
    }

    private void HandleMessage(ReadOnlySpan<byte> data)
    {
        if (Interlocked.Decrement(ref _eventCount) > 0)
        {
            return;
        }

        _gate.Set();
    }
}