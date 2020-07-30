using System.Collections.Generic;
using System.Reflection;
using Annium.Configuration.Tests;
using Annium.Testing;
using Microsoft.Extensions.DependencyModel;
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
            var result = Helper.BuildConfiguration<Config>(builder => builder.Add(cfg));

            // assert
            result.IsNotDefault();
            result.Plain.IsEqual(10);
            result.Abstract.IsNotDefault();
            result.Abstract.As<ConfigOne>().Type.IsEqual("ConfigOne");
            result.Abstract.As<ConfigOne>().Value.IsEqual(14U);
        }
    }
}