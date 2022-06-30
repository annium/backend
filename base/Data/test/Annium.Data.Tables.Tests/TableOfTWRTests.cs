using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Annium.Core.Mapper;
using Annium.Core.Primitives;
using Annium.Core.Primitives.Threading.Tasks;
using Annium.Testing;
using Annium.Testing.Lib;
using Xunit;

namespace Annium.Data.Tables.Tests;

public class TableOfTWRTests : TestBase
{
    [Fact]
    public async Task Works()
    {
        // arrange
        var mapper = Get<IMapper>();
        var ctx = new Context();
        var table = Table.New<Sample, SampleDto>()
            .Allow(TablePermission.All)
            .Key(x => x.Key)
            .Keep(x => x.IsAlive)
            .MapWith(x => new Sample(x.Key, x.IsAlive, ctx.Secret))
            .Build(mapper);
        ctx.Secret = "asd";
        var log1 = new List<IChangeEvent<Sample>>();
        var log2 = new List<IChangeEvent<Sample>>();

        // subscribe will emit init events
        table.Subscribe(log1.Add);
        table.Subscribe(log2.Add);
        log1.Has(1);
        log1.At(0).IsEqual(ChangeEvent.Init(Array.Empty<Sample>()));
        log2.At(0).IsEqual(log1.At(0));

        // init with some data
        var initValues = new[] { new SampleDto(1, true) };
        table.Init(initValues);
        await Wait.UntilAsync(() => log1.Count > 1);
        var data = table.ToArray();
        data.Has(1);
        var item = data.At(0);
        item.Key.Is(1);
        item.IsAlive.Is(true);
        item.Secret.Is(ctx.Secret);
        log1.Has(2);
        log1.At(1).IsEqual(ChangeEvent.Init(new[] { item }));
        log2.At(1).Is(log1.At(1));
    }

    private record Sample(int Key, bool IsAlive, string Secret) : ICopyable<Sample>
    {
        public Sample Copy() => this with { };
    }

    private record SampleDto(int Key, bool IsAlive);

    private record Context
    {
        public string Secret { get; set; } = string.Empty;
    }

    private class TableProfile : Profile
    {
        public TableProfile()
        {
            Map<(SampleDto, Context), Sample>(x => new Sample(x.Item1.Key, x.Item1.IsAlive, x.Item2.Secret));
        }
    }
}