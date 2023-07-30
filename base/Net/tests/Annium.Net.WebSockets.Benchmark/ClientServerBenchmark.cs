using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using BenchmarkDotNet.Attributes;

namespace Annium.Net.WebSockets.Benchmark;

[MemoryDiagnoser]
public class ClientServerBenchmark
{
    private const string Message = "{{\"stream\":\"{0}@aggTrade\",\"data\":{{\"e\":\"aggTrade\",\"E\":1689659049498,\"s\":\"{1}\",\"a\":2675370021,\"p\":\"30048.53000000\",\"q\":\"0.00332000\",\"f\":3174123265,\"l\":3174123265,\"T\":1689659049497,\"m\":false,\"M\":true}}}}";
    private static readonly ReadOnlyMemory<byte> MessageData = Encoding.UTF8.GetBytes(Message).AsMemory();


    [Params(100_000)]
    public long TotalMessages { get; set; }

    private CancellationTokenSource _cts = default!;
    private ManualResetEventSlim _gate;
    private long _eventCount;
    private IBackgroundExecutor _executor = default!;
    private Task _serverTask = default!;
    private System.Net.WebSockets.ClientWebSocket _socket = default!;
    private ManagedWebSocket _client = default!;
    private Task<WebSocketCloseStatus> _clientTask = default!;

    [IterationSetup]
    public void IterationSetup()
    {
        _cts = new();
        _gate = new ManualResetEventSlim();
        _eventCount = TotalMessages;

        _executor = Executor.Background.Parallel<WebSocketServer>();
        _executor.Start();

        // setup server
        var server = new WebSocketServer(new IPEndPoint(IPAddress.Loopback, 9898));
        server.OnConnected += ws => _executor.Schedule(async () => await HandleClient(new ManagedWebSocket(ws)));
        _serverTask = Task.Run(() => server.RunAsync(_cts.Token));

        // setup client
        _socket = new System.Net.WebSockets.ClientWebSocket();
        _client = new ManagedWebSocket(_socket);
        _client.TextReceived += HandleMessage;
        _socket.ConnectAsync(new Uri("ws://127.0.0.1:9898/"), CancellationToken.None).GetAwaiter().GetResult();
        _clientTask = Task.Run(() => _client.ListenAsync(_cts.Token));
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        _cts.Cancel();
        _executor.DisposeAsync().GetAwaiter().GetResult();
        Task.WhenAll(_serverTask, _clientTask);
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

    private async Task HandleClient(ManagedWebSocket clientSocket)
    {
        for (var i = 0; i < TotalMessages; i++)
            await clientSocket.SendTextAsync(MessageData);
    }
}