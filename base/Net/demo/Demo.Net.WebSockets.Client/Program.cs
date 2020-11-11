using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Core.Runtime.Types;
using Annium.Net.WebSockets;
using Annium.Serialization.Json;

namespace Demo.Net.WebSockets.Client
{
    public class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var typeManager = TypeManager.GetInstance(typeof(Program).Assembly, false);
            var serializer = ByteArraySerializer.Configure(opts => opts.ConfigureDefault(typeManager));
            var socket = new ClientWebSocket(serializer);

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