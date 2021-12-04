using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Pooling;
using Annium.Logging.Abstractions;

namespace Demo.Extensions.Pooling;

public class Program
{
    private const string Created = "Created";
    private const string Suspended = "Suspended";
    private const string Resumed = "Resumed";
    private const string Disposed = "Disposed";

    private static async Task Run(
        IServiceProvider provider,
        string[] args,
        CancellationToken ct
    )
    {
        var cache = CreateCache();

        // act
        await using var reference = await cache.GetAsync(0);
        Console.Write("Action with cache entry");
    }

    public static Task<int> Main(string[] args) => new Entrypoint()
        .Run(Run, args);


    private static IObjectCache<uint, Item> CreateCache()
    {
        var container = new ServiceContainer();
        container.AddTime().WithRealTime().SetDefault();

        var logger = container
            .AddLogging(route => route.UseConsole())
            .BuildServiceProvider()
            .Resolve<ILogger<ObjectCache<uint, Item>>>();

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

        return cache;
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