using System.Collections.Generic;
using Annium.Testing;

namespace Annium.Configuration.Abstractions.Tests
{
    public class ConfigurationBuilderTest
    {
        [Fact]
        public void BaseBuilding_Works()
        {
            // arrange
            var cfg = new Dictionary<string[], string>();
            cfg[new [] { "plain" }] = "10";
            cfg[new [] { "abstract", "type" }] = "ConfigOne";
            cfg[new [] { "abstract", "value" }] = "14";
            var builder = new ConfigurationBuilder();
            builder.Add(cfg);

            // act
            var result = builder.Build<Config>();

            // assert
            result.IsNotDefault();
            result.Plain.IsEqual(10);
            result.Abstract.IsNotDefault();
            result.Abstract.As<ConfigOne>().Type.IsEqual("ConfigOne");
            result.Abstract.As<ConfigOne>().Value.IsEqual(14U);
        }
    }
}