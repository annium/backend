using System;
using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperStructTests : MapperStructTestsBase
{
    public MapperStructTests() : base(new TestProvider())
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