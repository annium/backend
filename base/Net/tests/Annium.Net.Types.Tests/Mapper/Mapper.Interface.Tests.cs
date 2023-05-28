using System;
using Annium.Net.Types.Tests.Base.Mapper;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperInterfaceTests : MapperInterfaceTestsBase
{
    public MapperInterfaceTests() : base(new TestProvider())
    {
    }

    [Theory]
    [InlineData(typeof(IEmptyInterface))]
    public void Empty(Type type)
    {
        Empty_Base(type);
    }
}

file interface IEmptyInterface
{
}