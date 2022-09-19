using System;
using System.Text.Json;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;

namespace Annium.Serialization.Json.Tests;

public class TestBase
{
    protected ISerializer<string> GetSerializer(Action<JsonSerializerOptions> configure)
    {
        var container = new ServiceContainer();
        container.AddRuntime(GetType().Assembly);
        container.AddTime().WithRealTime().SetDefault();
        container.AddJsonSerializers()
            .Configure(opts =>
            {
                opts.UseCamelCaseNamingPolicy();
                configure(opts);
            })
            .SetDefault();
        container.AddLogging();

        var provider = container.BuildServiceProvider();

        provider.UseLogging(x => x.UseInMemory());

        return provider.ResolveSerializer<string>(Abstractions.Constants.DefaultKey, Constants.MediaType);
    }

    protected ISerializer<string> GetSerializer() => GetSerializer(delegate { });
}