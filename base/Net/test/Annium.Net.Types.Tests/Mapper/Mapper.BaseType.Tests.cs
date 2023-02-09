using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperBaseTypeTests : TestBase
{
    [Fact]
    public void BaseType()
    {
        // arrange
        var target = typeof(int).ToContextualType();

        // act
        var model = Map(target).As<StructModel>();

        // assert
        model.Namespace.Is(target.GetNamespace());
        model.Name.Is("int");
    }
}