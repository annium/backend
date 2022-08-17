using System;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Logging.Abstractions;
using Annium.Net.WebSockets;

await using var entry = Entrypoint.Default.Setup();

var (provider, ct) = entry;

var socket = new ClientWebSocket(provider.Resolve<ILoggerFactory>());

await socket.ConnectAsync(new Uri("ws://localhost:5000/ws/data"), ct);

if (ct.IsCancellationRequested)
{
    Console.WriteLine("Connection terminated");
    return;
}

Console.WriteLine("Connection established");

var tcs = new TaskCompletionSource<object>();

ct.Register(() =>
{
    Console.WriteLine("Process terminated");
    tcs.TrySetResult(new object());
});

var s1 = socket.ListenText().Subscribe(x => Console.WriteLine($"In: '{x}'"));

await tcs.Task;
s1.Dispose();