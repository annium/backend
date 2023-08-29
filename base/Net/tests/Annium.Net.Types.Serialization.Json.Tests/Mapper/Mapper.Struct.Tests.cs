using System;
using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;
using Xunit.Abstractions;

namespace Annium.Net.Types.Serialization.Json.Tests.Mapper;

public class MapperStructTests : MapperStructTestsBase
{
    public MapperStructTests(
        ITestOutputHelper outputHelper
    ) : base(
        new TestProvider(),
        outputHelper
    )
    {
    }

    [Theory]
    [InlineData(typeof(EmptyStruct))]
    [InlineData(typeof(EmptyRecord))]
    public void Empty(Type type)
    {
        Empty_Base(type);
    }

    [Fact]
    public void Struct()
    {
        Struct_Base();
    }
}

file struct EmptyStruct
{
}

file record EmptyRecord;