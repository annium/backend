using System.Reflection;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Core.Mapper.Tests;

public class ContextualProfileTest
{
    [Fact]
    public void ContextualMapping_With_Works()
    {
        // arrange
        var mapper = GetMapper();
        var payload = new OuterPayload(InnerPayload.B);

        // act
        var result = mapper.Map<OuterModel>(payload);

        // assert
        result.As<OuterModel>().X.Is(InnerModel.D);
    }

    [Fact]
    public void ContextualMapping_Field_Works()
    {
        // arrange
        var mapper = GetMapper();
        var payload = new OuterPayload(InnerPayload.B);

        // act
        var result = mapper.Map<OuterModel>(payload);

        // assert
        result.As<OuterModel>().X.Is(InnerModel.D);
    }

    private IMapper GetMapper() => new ServiceContainer()
        .AddRuntime(Assembly.GetCallingAssembly())
        .AddMapper(autoload: false)
        .AddProfile<ContextualProfile>()
        .BuildServiceProvider()
        .Resolve<IMapper>();

    private class ContextualProfile : Profile
    {
        public ContextualProfile()
        {
            Map<SomePayload, SomeModel>().For<InnerModel>(x => x.X, ctx => x => ctx.Map<InnerModel>(x));
            Map<InnerPayload, InnerModel>(x => x == InnerPayload.A ? InnerModel.C : InnerModel.D);
            Map<OuterPayload, OuterModel>(ctx => x => new OuterModel(ctx.Map<InnerModel>(x.X)));
        }
    }

    private record SomePayload(InnerPayload X, int Value);

    private record SomeModel(InnerModel X, int Value);

    private record OuterPayload(InnerPayload X);

    private record OuterModel(InnerModel X);

    private enum InnerPayload
    {
        A,
        B
    }

    private enum InnerModel
    {
        C,
        D
    }
}