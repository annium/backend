using System.Threading.Tasks;
using Annium.Net.Types.Extensions;
using Annium.Net.Types.Models;
using Annium.Testing;
using Namotion.Reflection;
using Xunit;

namespace Annium.Net.Types.Tests.Mapper;

public class MapperSpecialTests : TestBase
{
    [Fact]
    public void Task_Generic_Nullable()
    {
        // arrange
        var target = typeof(Task<string?>).ToContextualType();

        // act
        var model = Map(target).As<NullableModel>().As<StructModel>();

        // assert
        model.Namespace.Is(typeof(string).GetNamespace());
        model.Name.Is($"{BaseType.String}?");
    }

    [Fact]
    public void Task_Generic_NotNullable()
    {
        // arrange
        var target = typeof(Task<string>).ToContextualType();

        // act
        var model = Map(target).As<NullableModel>().As<StructModel>();

        // assert
        model.Namespace.Is(typeof(string).GetNamespace());
        model.Name.Is(BaseType.String);
    }
}