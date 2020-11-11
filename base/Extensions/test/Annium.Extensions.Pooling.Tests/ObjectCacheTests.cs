using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Logging.Abstractions;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Annium.Extensions.Pooling.Tests
{
    public class ObjectCacheTests
    {
        private const string Created = "Created";
        private const string Suspended = "Suspended";
        private const string Resumed = "Resumed";
        private const string Disposed = "Disposed";

        [Fact]
        public async Task ObjectCache_Create_Works()
        {
            // arrange
            var (cache, logs) = CreateCache();

            // act
            var references = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => cache.GetAsync(0)));

            // assert
            references.Has(10);
            references.All(x => x.Value != null).IsTrue();
            references.GroupBy(x => x.Value).Has(1);
            logs.Has(1);
            logs.ElementAt(0).IsEqual($"0 {Created}");
        }

        [Fact]
        public async Task ObjectCache_Suspend_Works()
        {
            // arrange
            var (cache, logs) = CreateCache();

            // act
            await Task.WhenAll(Enumerable.Range(0, 30).Select(async i =>
            {
                await using var reference = await cache.GetAsync((uint) i % 2);
                await Task.Delay(20);
            }));
            var references = await Task.WhenAll(Enumerable.Range(0, 20).Select(i => cache.GetAsync((uint) i % 2)));
            await cache.DisposeAsync();

            // assert
            references.Has(20);
            references.GroupBy(x => x.Value).Has(2);
            logs.Count.IsNotDefault();
            Enumerable.Range(0, 2).Select(x => $"{x} {Created}").All(logs.Contains).IsTrue();
            foreach (var i in Enumerable.Range(0, 2))
            {
                logs.Contains($"{i} {Suspended}").IsTrue();
                logs.Contains($"{i} {Resumed}").IsTrue();
            }

            Enumerable.Range(0, 2).Select(x => $"{x} {Disposed}").All(logs.Contains).IsTrue();
        }

        private (IObjectCache<uint, Item>, IReadOnlyCollection<string>) CreateCache()
        {
            var logger = new ServiceCollection()
                .AddSingleton<Func<Instant>>(SystemClock.Instance.GetCurrentInstant)
                .AddLogging(route => route.UseInMemory())
                // .AddLogging(route => route.UseConsole())
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
            private readonly uint _id;
            private readonly Action<string> _log;

            public Item(
                uint id,
                Action<string> log
            )
            {
                _id = id;
                _log = log;
                log($"{id} {Created}");
            }

            public async Task Suspend()
            {
                await Task.Delay(10);
                _log($"{_id} {Suspended}");
            }

            public async Task Resume()
            {
                await Task.Delay(10);
                _log($"{_id} {Resumed}");
            }

            public void Dispose()
            {
                _log($"{_id} {Disposed}");
            }
        }
    }
}