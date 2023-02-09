using Annium.Net.Types.Extensions;
using Annium.Net.Types.Internal.Map;
using Annium.Net.Types.Models;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internal.Mapper;

public class MapBaseTypeTests
{
    [Fact]
    public void BaseType()
    {
        // arrange
        var target = typeof(int).ToContextualType();

        // act
        var model = Map.ToModel(target).As<StructModel>();

        // assert
        model.Namespace.Is(target.GetNamespace());
        model.Name.Is("int");
    }
}