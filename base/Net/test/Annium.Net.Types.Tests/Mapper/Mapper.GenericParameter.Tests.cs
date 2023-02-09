using System.Collections.Generic;
using Annium.Net.Types.Internal.Mappers;
using Annium.Net.Types.Models;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperGenericParameterTests : TestBase
{
    [Fact]
    public void GenericParameter()
    {
        // arrange
        var target = typeof(List<>).GetGenericArguments()[0].ToContextualType();

        // act
        var model = Map(target).As<NullableModel>().Type.As<GenericParameterModel>();

        // assert
        model.Name.Is(target.Type.Name);
    }
}