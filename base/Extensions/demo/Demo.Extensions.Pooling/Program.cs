using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Pooling;

await using var entry = Entrypoint.Default.Setup();

var cache = CreateCache();

// act
await using var reference = await cache.GetAsync(0);
Console.Write("Action with cache entry");


IObjectCache<uint, Item> CreateCache()
{
    var container = new ServiceContainer();
    container.AddTime().WithRealTime().SetDefault();

    // ReSharper disable once CollectionNeverQueried.Local
    var logs = new List<string>();

    void Log(string message)
    {
        lock (logs) logs.Add(message);
    }

    var sp = container
        .AddLogging()
        .AddObjectCache<uint, Item, ItemProvider>(ServiceLifetime.Singleton)
        .Add<Action<string>>(Log).AsSelf().Singleton()
        .BuildServiceProvider()
        .UseLogging(route => route.UseConsole());
    // ReSharper disable once VariableHidesOuterVariable
    var cache = sp.Resolve<IObjectCache<uint, Item>>();

    return cache;
}

class ItemProvider : ObjectCacheProvider<uint, Item>
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

class Item : IDisposable
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
        log($"{id} Created");
    }

    public async Task Suspend()
    {
        await Task.Delay(10);
        _log($"{_id} Suspended");
    }

    public async Task Resume()
    {
        await Task.Delay(10);
        _log($"{_id} Resumed");
    }

    public void Dispose()
    {
        _log($"{_id} Disposed");
    }
}