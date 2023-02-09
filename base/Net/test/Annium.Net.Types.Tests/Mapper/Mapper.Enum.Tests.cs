using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperEnumTests : TestBase
{
    [Fact]
    public void Enum()
    {
        // arrange
        var target = typeof(SimpleEnum).ToContextualType();

        // act
        var modelRef = Map(target);

        // assert
        modelRef.Namespace.Is(typeof(SimpleEnum).GetNamespace().ToString());
        modelRef.Name.Is(nameof(SimpleEnum));
        Models.Has(1);
        var model = Models.At(0).As<EnumModel>();
        model.Namespace.Is(typeof(SimpleEnum).GetNamespace());
        model.Name.Is(nameof(SimpleEnum));
        model.Values.Has(2);
        model.Values.At("A").Is(1);
        model.Values.At("B").Is(3);
    }
}

file enum SimpleEnum
{
    A = 1,
    B = 3
}