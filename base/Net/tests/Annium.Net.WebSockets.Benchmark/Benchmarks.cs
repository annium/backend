using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using BenchmarkDotNet.Attributes;

namespace Annium.Net.WebSockets.Benchmark;

[MemoryDiagnoser]
public class Benchmarks
{
    private CancellationTokenSource _cts = default!;
    private ManualResetEventSlim _gate = default!;
    private long _eventCount;
    private IBackgroundExecutor _executor = default!;
    private System.Net.WebSockets.ClientWebSocket _socket = default!;
    private ManagedWebSocket _client = default!;
    private Task<WebSocketCloseStatus> _clientTask = default!;

    [IterationSetup]
    public void IterationSetup()
    {
        _cts = new();
        _gate = new ManualResetEventSlim();
        _eventCount = Constants.TotalMessages;

        _executor = Executor.Background.Parallel<WebSocketServer>();
        _executor.Start();

        _socket = new System.Net.WebSockets.ClientWebSocket();
        _client = new ManagedWebSocket(_socket);
        _client.TextReceived += HandleMessage;
        _socket.ConnectAsync(new Uri($"ws://127.0.0.1:{Constants.Port}/"), CancellationToken.None).GetAwaiter().GetResult();
        _clientTask = Task.Run(() => _client.ListenAsync(_cts.Token));
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        _cts.Cancel();
        _clientTask.Wait();
    }

    [Benchmark]
    public void Receive()
    {
        _gate.Wait();
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