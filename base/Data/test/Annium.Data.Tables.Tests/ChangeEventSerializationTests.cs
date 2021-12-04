using AgileObjects.NetStandardPolyfills;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Tables.Tests;

public class ChangeEventSerializationTests
{
    [Fact]
    public void InitEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Init(new[] { 1, 2 });

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        var n = back.As<InitEvent<int>>();
        n.Values.IsEqual(new[] { 1, 2 });
    }

    [Fact]
    public void AddEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Add(3);

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        back.As<AddEvent<int>>().Value.Is(3);
    }

    [Fact]
    public void UpdateEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Update(2, 3);

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        back.As<UpdateEvent<int>>().OldValue.Is(2);
        back.As<UpdateEvent<int>>().NewValue.Is(3);
    }

    [Fact]
    public void DeleteEvent_Ok()
    {
        // arrange
        var serializer = GetSerializer();
        var e = ChangeEvent.Delete(3);

        // act
        var back = serializer.Deserialize<IChangeEvent<int>>(serializer.Serialize(e));

        // assert
        back.As<DeleteEvent<int>>().Value.Is(3);
    }

    private ISerializer<string> GetSerializer()
    {
        var container = new ServiceContainer();
        container.AddRuntimeTools(GetType().GetAssembly(), true);
        container.AddTime().WithRealTime().SetDefault();
        container.AddJsonSerializers().SetDefault();
        container.AddLogging(x => x.UseInMemory());

        var sp = container.BuildServiceProvider();

        return sp.Resolve<ISerializer<string>>();
    }
}