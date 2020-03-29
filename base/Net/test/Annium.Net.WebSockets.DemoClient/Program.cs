using System;
using System.Reactive.Linq;
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

            await socket.ConnectAsync(new Uri("wss://echo.websocket.org"), CancellationToken.None);

            var s1 = socket.ListenText().Subscribe(x => { Console.WriteLine($"S1:{x}"); });
            var s2 = socket.ListenText().Subscribe(x => { Console.WriteLine($"S2:{x}"); });


            await socket.Send("Hello", CancellationToken.None);
            await socket.Send("World", CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(3));
            s1.Dispose();
            s2.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}