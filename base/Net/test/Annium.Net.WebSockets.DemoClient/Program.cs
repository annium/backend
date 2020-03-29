using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;

namespace Annium.Net.WebSockets.DemoClient
{
    public class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var socket = new ClientWebSocket(Serializers.Json);

            await socket.ConnectAsync(new Uri("ws://localhost:5000/ws/data"), token);

            if (token.IsCancellationRequested)
            {
                Console.WriteLine("Connection terminated");
                return;
            }

            Console.WriteLine("Connection established");

            var tcs = new TaskCompletionSource<object>();

            token.Register(() =>
            {
                Console.WriteLine("Process terminated");
                tcs.SetResult(new object());
            });

            var s1 = socket.ListenText().Subscribe(x => Console.WriteLine($"In: '{x}'"));

            await tcs.Task;
            s1.Dispose();
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}