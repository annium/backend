using System.Text.Json.Serialization;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using Annium.Testing;
using Xunit;

namespace Annium.Serialization.Json.Tests;

public class ConfigurationTest
{
    [Fact]
    public void MultipleConfigurations_Work()
    {
        // arrange
        var container = new ServiceContainer();
        container.AddRuntimeTools(GetType().Assembly, false);
        container.AddTime().WithRealTime().SetDefault();
        container.AddLogging(x => x.UseInMemory());
        // default
        container.AddJsonSerializers().SetDefault();
        // custom
        container.AddJsonSerializers("a").Configure(x =>
        {
            x.UseCamelCaseNamingPolicy();
            x.NumberHandling = JsonNumberHandling.WriteAsString;
        });
        container.AddJsonSerializers("b").Configure(x => x.UseCamelCaseNamingPolicy());
        var sp = container.BuildServiceProvider();

        var serializerDefault = sp.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
        var serializerA = sp.ResolveSerializer<string>("a", Constants.MediaType);
        var serializerB = sp.ResolveSerializer<string>("b", Constants.MediaType);
        var sample = new { X = 1 };

        // act
        var resultDefault = serializerDefault.Serialize(sample);
        var resultA = serializerA.Serialize(sample);
        var resultB = serializerB.Serialize(sample);

        // assert
        sp.Resolve<ISerializer<string>>().Is(serializerDefault);
        resultDefault.IsEqual(@"{""X"":1}");
        resultA.IsEqual(@"{""x"":""1""}");
        resultB.IsEqual(@"{""x"":1}");
    }
}