using Annium.Net.Types.Internal.Map;
using Annium.Net.Types.Models;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Internal.Mapper;

public class MapNullableTests
{
    [Fact]
    public void Nullable_Struct()
    {
        // arrange
        var target = typeof(int?).ToContextualType();

        // act
        var model = Map.ToModel(target).As<NullableModel>();

        // assert
        model.Name.Is($"{BaseType.Int}?");
    }

    [Fact]
    public void Nullable_Class()
    {
        // arrange
        var target = typeof(RecordWithNullable).GetProperty(nameof(RecordWithNullable.Value))!.ToContextualProperty().PropertyType;

        // act
        var model = Map.ToModel(target).As<NullableModel>();

        // assert
        model.Name.Is($"{BaseType.String}?");
    }
}

file record RecordWithNullable(string? Value);