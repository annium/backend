using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Annium.Extensions.Execution;
using Annium.Net.WebSockets.Benchmark;
using Annium.Threading.Tasks;
using ClientWebSocket = System.Net.WebSockets.ClientWebSocket;

Trace("start");

await using var executor = Executor.Background.Parallel<WebSocketServer>();
executor.Start();

var cts = new CancellationTokenSource();

// server
var server = new WebSocketServer(new IPEndPoint(IPAddress.Loopback, 9898));
server.OnConnected += ws => executor.Schedule(async () => await HandleClient(new ManagedWebSocket(ws)));
var serverRunTask = Task.Run(() => server.RunAsync(cts.Token));

// client
var socket = new ClientWebSocket();
var client = new ManagedWebSocket(socket);
var messageCount = 1_000_000L;
var counter = messageCount;
client.TextReceived += _ => Interlocked.Decrement(ref counter);
await socket.ConnectAsync(new Uri("ws://127.0.0.1:9898/"), CancellationToken.None);
var clientListenTask = Task.Run(() => client.ListenAsync(cts.Token));
await Task.Delay(10);
Trace("client: send messages");
ReadOnlyMemory<byte> msg = "demo"u8.ToArray().AsMemory();
for (var i = 0; i < messageCount; i++)
    await client.SendTextAsync(msg);


Trace("client: wait for all messages returned");
await Wait.UntilAsync(() => counter == 0);

Trace("client: disconnect");
await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);

cts.Cancel();
await Task.WhenAll(clientListenTask, serverRunTask);

Trace("done");


static async Task HandleClient(ManagedWebSocket clientSocket)
{
    // create channel to decouple read/write flow
    var channel = Channel.CreateUnbounded<ReadOnlyMemory<byte>>();

    // pass incoming messages to channel writer
    clientSocket.TextReceived += text => channel.Writer.TryWrite(text.ToArray());

    // start echo task
    var echoCts = new CancellationTokenSource();
    var echoTask = Task.Run(() => RunEchoAsync(channel.Reader, clientSocket, echoCts.Token));

    // listen till end
    await clientSocket.ListenAsync(CancellationToken.None);

    // mark writer complete when client is disconnected
    channel.Writer.Complete();

    // cancel and wait for echo task
    echoCts.Cancel();
    await echoTask;
}

static async Task RunEchoAsync(ChannelReader<ReadOnlyMemory<byte>> reader, ManagedWebSocket clientSocket, CancellationToken ct)
{
    try
    {
        while (!ct.IsCancellationRequested)
        {
            var data = await reader.ReadAsync(ct);
            await clientSocket.SendTextAsync(data, ct);
        }

        while (true)
        {
            if (reader.TryRead(out var data))
            {
                await clientSocket.SendTextAsync(data, ct);
            }
            else
            {
                break;
            }
        }
    }
    catch (ChannelClosedException)
    {
    }
    catch (OperationCanceledException)
    {
    }
}

static void Trace(string msg)
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {msg}");
}
// using BenchmarkDotNet.Columns;
// using BenchmarkDotNet.Configs;
// using BenchmarkDotNet.Diagnosers;
// using BenchmarkDotNet.Engines;
// using BenchmarkDotNet.Environments;
// using BenchmarkDotNet.Exporters;
// using BenchmarkDotNet.Jobs;
// using BenchmarkDotNet.Loggers;
// using BenchmarkDotNet.Running;
// using BenchmarkDotNet.Validators;
//
// var config = new ManualConfig()
//     .AddExporter(MarkdownExporter.Default)
//     .AddDiagnoser(MemoryDiagnoser.Default)
//     .AddJob(Job.Default
//         .WithWarmupCount(0)
//         .WithLaunchCount(1)
//         .WithIterationCount(3)
//         .WithStrategy(RunStrategy.Throughput)
//         .WithPlatform(Platform.X64)
//         .WithRuntime(CoreRuntime.Core70))
//     .AddValidator(JitOptimizationsValidator.DontFailOnError)
//     .AddLogger(ConsoleLogger.Default)
//     .AddColumnProvider(DefaultColumnProviders.Instance);
//
// BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);