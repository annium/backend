using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Testing;

namespace Annium.Extensions.Pooling.Tests
{
    public class ObjectCacheTests
    {
        private const string created = "created";
        private const string suspended = "suspended";
        private const string resumed = "resumed";
        private const string disposed = "disposed";

        [Fact]
        public async Task ObjectCache_Create_Works()
        {
            // arrange
            var (cache, logs) = CreateCache();

            // act
            var references = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => cache.GetAsync(0)));

            // assert
            references.Has(100);
            references.GroupBy(x => x.Value).Has(1);
            logs.Has(1);
            logs.ElementAt(0).IsEqual($"0 {created}");
        }

        [Fact]
        public async Task ObjectCache_Suspend_Works()
        {
            // arrange
            var (cache, logs) = CreateCache();

            // act
            await Task.WhenAll(Enumerable.Range(0, 10000).Select(async i =>
            {
                using (var reference = await cache.GetAsync((uint)i % 2))
                {
                    await Task.Delay(20);
                }
            }));
            await Task.Delay(10);
            var references = await Task.WhenAll(Enumerable.Range(0, 2000).Select(i => cache.GetAsync((uint)i % 2)));
            cache.Dispose();

            // assert
            foreach (var log in logs) Console.WriteLine(log);
            references.Has(2000);
            references.GroupBy(x => x.Value).Has(2);
            logs.Count.IsNotDefault();
            Enumerable.Range(0, 2).Select(x => $"{x} {created}").All(logs.Contains).IsTrue();
            foreach (var i in Enumerable.Range(0, 2))
                if (logs.Contains($"{i} {suspended}"))
                    logs.Contains($"{i} {resumed}").IsTrue();
            Enumerable.Range(0, 2).Select(x => $"{x} {disposed}").All(logs.Contains).IsTrue();
        }

        private (IObjectCache<uint, Item>, IReadOnlyCollection<string>) CreateCache()
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