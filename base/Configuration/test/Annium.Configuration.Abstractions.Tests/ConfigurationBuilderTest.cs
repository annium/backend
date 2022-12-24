using System.Collections.Generic;
using Annium.Configuration.Tests;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests;

public class ConfigurationBuilderTest
{
    [Fact]
    public void BaseBuilding_Works()
    {
        // arrange
        var cfg = new Dictionary<string[], string>();
        cfg[new[] { "plain" }] = "10";
        cfg[new[] { "abstract", "type" }] = "ConfigOne";
        cfg[new[] { "abstract", "value" }] = "14";
        cfg[new[] { "enum" }] = "two";

        // act
        var provider = Helper.GetProvider<Config>(x => x.Add(cfg));
        var result = provider.Resolve<Config>();
        var nested = provider.Resolve<SomeConfig>();

        // assert
        result.IsNotDefault();
        result.Plain.Is(10);
        result.Abstract.IsEqual(nested);
        result.Enum.Is(SomeEnum.Two);
        nested.IsEqual(new ConfigOne { Value = 14 });
    }
}