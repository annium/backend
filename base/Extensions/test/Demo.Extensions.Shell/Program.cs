using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using Annium.Extensions.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Extensions.Shell
{
    public class Program
    {
        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var shell = provider.GetRequiredService<IShell>();
            var ls = await shell
                .Cmd("ls")
                .Configure(new ProcessStartInfo() { WorkingDirectory = "/" })
                .RunAsync(token);

            Console.WriteLine(ls.IsSuccess);
            Console.WriteLine(ls.Code);
            Console.WriteLine(ls.Output);
            Console.WriteLine(ls.Error);
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}