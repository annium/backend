using System.Collections.Generic;
using Annium.Configuration.Tests;
using Annium.Data.Models.Extensions;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Configuration.Abstractions.Tests
{
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

            // act
            var provider = Helper.GetProvider<Config>(builder => builder.Add(cfg));
            var result = provider.GetRequiredService<Config>();
            var nested = provider.GetRequiredService<SomeConfig>();

            // assert
            result.IsNotDefault();
            result.Plain.IsEqual(10);
            result.Abstract.IsEqual(nested);
            nested.IsShallowEqual(new ConfigOne { Value = 14 });
        }
    }
}