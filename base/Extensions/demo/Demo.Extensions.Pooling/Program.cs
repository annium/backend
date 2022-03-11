using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Pooling;

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

        var logs = new List<string>();

        void Log(string message)
        {
            lock (logs!) logs.Add(message);
        }

        var sp = container
            .AddLogging()
            .AddObjectCache<uint, Item, ItemProvider>(ServiceLifetime.Singleton)
            .Add<Action<string>>(Log).AsSelf().Singleton()
            .BuildServiceProvider()
            .UseLogging(route => route.UseConsole());
        var cache = sp.Resolve<IObjectCache<uint, Item>>();

        return cache;
    }

    private class ItemProvider : ObjectCacheProvider<uint, Item>
    {
        private readonly Action<string> _log;
        public override bool HasCreate => true;
        public override bool HasExternalCreate => false;

        public ItemProvider(
            Action<string> log
        )
        {
            _log = log;
        }

        public override async Task<Item> CreateAsync(uint id, CancellationToken ct)
        {
            await Task.Delay(10);
            return new Item(id, _log);
        }

        public override Task SuspendAsync(Item value) => value.Suspend();
        public override Task ResumeAsync(Item value) => value.Resume();
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