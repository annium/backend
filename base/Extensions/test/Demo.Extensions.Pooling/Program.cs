using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.Entrypoint;
using Annium.Extensions.Pooling;

namespace Demo.Extensions.Pooling
{
    public class Program
    {
        private const string created = "created";
        private const string suspended = "suspended";
        private const string resumed = "resumed";
        private const string disposed = "disposed";

        private static async Task Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var (cache, logs) = CreateCache();

            // act
            using (var reference = await cache.GetAsync(0))
            {
                Console.Write("Action with cache entry");
            }
        }

        public static Task<int> Main(string[] args) => new Entrypoint()
            .Run(Run, args);


        private static (IObjectCache<uint, Item>, IReadOnlyCollection<string>) CreateCache()
        {
            var logs = new List<string>();
            void log(string message)
            {
                lock (logs) logs.Add(message);
            }
            var cache = new ObjectCache<uint, Item>(
                async id =>
                {
                    await Task.Delay(10);
                    return new Item(id, log);
                },
                item => item.Suspend(),
                item => item.Resume()
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
                log($"{id} {created}");
            }

            public async Task Suspend()
            {
                await Task.Delay(10);
                log($"{id} {suspended}");
            }

            public async Task Resume()
            {
                await Task.Delay(10);
                log($"{id} {resumed}");
            }

            public void Dispose()
            {
                log($"{id} {disposed}");
            }
        }
    }
}