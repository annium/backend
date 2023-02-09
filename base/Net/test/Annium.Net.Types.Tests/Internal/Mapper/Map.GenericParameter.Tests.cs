using System.Collections.Generic;
using Annium.Net.Types.Internal.Map;
using Annium.Net.Types.Models;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internal.Mapper;

public class MapGenericParameterTests
{
    [Fact]
    public void GenericParameter()
    {
        // arrange
        var target = typeof(List<>).GetGenericArguments()[0].ToContextualType();

        // act
        var model = Map.ToModel(target).As<NullableModel>().Type.As<GenericParameterModel>();

        // assert
        model.Name.Is(target.Type.Name);
    }
}