using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Pooling;
using Annium.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Demo.Extensions.Pooling
{
    public class Program
    {
        private const string Created = "Created";
        private const string Suspended = "Suspended";
        private const string Resumed = "Resumed";
        private const string Disposed = "Disposed";

        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var (cache, logs) = CreateCache();

            // act
            await using var reference = await cache.GetAsync(0);
            Console.Write("Action with cache entry");
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .Run(Run, args);


        private static (IObjectCache<uint, Item>, IReadOnlyCollection<string>) CreateCache()
        {
            var logger = new ServiceCollection()
                .AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant)
                .AddLogging(route => route.UseConsole())
                .BuildServiceProvider()
                .GetRequiredService<ILogger<ObjectCache<uint, Item>>>();

            var logs = new List<string>();

            void Log(string message)
            {
                lock (logs!) logs.Add(message);
            }

            var cache = new ObjectCache<uint, Item>(
                async id =>
                {
                    await Task.Delay(10);
                    return new Item(id, Log);
                },
                item => item.Suspend(),
                item => item.Resume(),
                logger
            );

            return (cache, logs);
        }

        private class Item : IDisposable
        {
            private readonly uint id;
            private readonly Action<string> log;

            public Item(
                uint id,
                Action<string> log
            )
            {
                this.id = id;
                this.log = log;
                log($"{id} {Created}");
            }

            public async Task Suspend()
            {
                await Task.Delay(10);
                log($"{id} {Suspended}");
            }

            public async Task Resume()
            {
                await Task.Delay(10);
                log($"{id} {Resumed}");
            }

            public void Dispose()
            {
                log($"{id} {Disposed}");
            }
        }
    }
}