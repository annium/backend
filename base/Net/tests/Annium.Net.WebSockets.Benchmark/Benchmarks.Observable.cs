using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Net.WebSockets.Benchmark.Internal;
using BenchmarkDotNet.Attributes;

namespace Annium.Net.WebSockets.Benchmark;

public partial class Benchmarks
{
    private CancellationTokenSource _observableCts = default!;
    private ManualResetEventSlim _observableGate = default!;
    private long _observableEventCount;
    private ClientWebSocket _observableSocket = default!;
    private Task<WebSocketCloseResult> _observableListenTask = default!;

    [IterationSetup(Target = nameof(Observable))]
    public void IterationSetup_Observable()
    {
        _observableCts = new();
        _observableGate = new ManualResetEventSlim();
        _observableEventCount = Constants.TotalMessages;

        _observableSocket = new ClientWebSocket();
        var client = new ManagedWebSocket(_observableSocket);
        client.ObserveText().Subscribe(HandleMessage_Observable);
        _observableSocket.ConnectAsync(new Uri($"ws://127.0.0.1:{Constants.Port}/"), CancellationToken.None).GetAwaiter().GetResult();
        _observableListenTask = client.ListenAsync(_observableCts.Token);
    }

    [IterationCleanup(Target = nameof(Observable))]
    public void IterationCleanup_Observable()
    {
        _observableSocket.CloseOutputAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        _observableCts.Cancel();
        _observableListenTask.Wait();
    }

    [Benchmark]
    public void Observable()
    {
        _observableGate.Wait();
    }

    private void HandleMessage_Observable(ReadOnlyMemory<byte> data)
    {
        if (Interlocked.Decrement(ref _observableEventCount) > 0)
        {
            return;
        }

        _observableGate.Set();
    }
}