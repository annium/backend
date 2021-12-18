using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Tests;

public class TestBase
{
    protected ISerializer<string> GetSerializer()
    {
        var container = new ServiceContainer();
        container.AddRuntimeTools(GetType().Assembly, false);
        container.AddTime().WithRealTime().SetDefault();
        container.AddJsonSerializers()
            .Configure(opts => opts.UseCamelCaseNamingPolicy())
            .SetDefault();
        container.AddLogging();

        var provider = container.BuildServiceProvider();

        provider.UseLogging(x => x.UseInMemory());

        return provider.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }
}