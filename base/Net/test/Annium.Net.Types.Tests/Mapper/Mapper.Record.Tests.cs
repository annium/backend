using System.Collections.Generic;
using System.Collections.Immutable;
using Annium.Net.Types.Refs;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperRecordTests : TestBase
{
    [Fact]
    public void Interface()
    {
        // arrange
        var target = typeof(IDictionary<,>).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<RecordRef>().Key
            .As<GenericParameterRef>().Name.Is("TKey");
        modelRef
            .As<RecordRef>().Value
            .As<GenericParameterRef>().Name.Is("TValue");
        Models.IsEmpty();
    }

    [Fact]
    public void Implementation()
    {
        // arrange
        var target = typeof(ImmutableDictionary<string, int>).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef
            .As<RecordRef>().Key
            .As<BaseTypeRef>().Name.Is(BaseType.String);
        modelRef
            .As<RecordRef>().Value
            .As<BaseTypeRef>().Name.Is(BaseType.Int);
        Models.IsEmpty();
    }
}