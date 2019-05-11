using System.Collections.Generic;
using Annium.Testing;

namespace Annium.Extensions.Configuration.Tests
{
    public class ConfigurationBuilderTest
    {
        [Fact]
        public void BaseBuilding_Works()
        {
            // arrange
            var cfg = new Dictionary<string, string>();
            cfg["value"] = "something";
            var builder = new ConfigurationBuilder();
            builder.Add(cfg);

            // act
            var result = builder.Build<Config>();

            // assert
            result.IsNotDefault();
            result.Value.IsEqual("something");
        }

        private class Config
        {
            public string Value { get; set; }
        }
    }
}