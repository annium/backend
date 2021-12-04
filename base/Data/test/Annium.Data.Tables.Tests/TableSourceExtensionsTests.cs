using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

public class TableSourceExtensionsTests
{
    [Fact]
    public async Task SyncAddRemove_Works()
    {
        // arrange
        var table = Table.New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Build();
        var initValues = new[] { new Sample(1, "a"), new Sample(2, "b") };
        table.Init(initValues);
        var log = new List<IChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log.Add);
        log.Has(1);
        log.At(0).IsEqual(ChangeEvent.Init(initValues));

        // sync with some data
        var syncValues = new[] { new Sample(1, "a"), new Sample(3, "c") };
        table.SyncAddDelete(syncValues);
        await Wait.UntilAsync(() => log.Count > 1);
        log.Has(3);
        log.At(1).IsEqual(ChangeEvent.Delete(initValues[1]));
        log.At(2).IsEqual(ChangeEvent.Add(syncValues[1]));
    }

    [Fact]
    public async Task SyncAddUpdateRemove_Works()
    {
        // arrange
        var table = Table.New<Sample>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Build();
        var initValues = new[] { new Sample(1, "a"), new Sample(2, "b") };
        table.Init(initValues);
        var log = new List<IChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log.Add);
        log.Has(1);
        log.At(0).IsEqual(ChangeEvent.Init(initValues));

        // sync with some data
        var syncValues = new[] { new Sample(1, "z"), new Sample(3, "c") };
        table.SyncAddUpdateDelete(syncValues);
        await Wait.UntilAsync(() => log.Count > 1);
        log.Has(4);
        log.At(1).IsEqual(ChangeEvent.Delete(initValues[1]));
        log.At(2).IsEqual(ChangeEvent.Update(initValues[0], syncValues[0]));
        log.At(3).IsEqual(ChangeEvent.Add(syncValues[1]));
    }

    private sealed record Sample(int Key, string Data) : ICopyable<Sample>
    {
        public Sample Copy() => this with { };
    }
}