using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Testing;
using YamlDotNet.Serialization;

namespace Annium.Extensions.Configuration.Tests
{
    public class YamlConfigurationProviderTest
    {
        [Fact]
        public void YamlConfiguration_Works()
        {
            // arrange
            var cfg = new Config();
            cfg.Flag = true;
            cfg.Plain = 7;
            cfg.Array = new [] { 4, 7 };
            cfg.Matrix = new List<int[]>() { new [] { 3, 2 }, new [] { 5, 4 } };
            cfg.List = new List<Val>() { new Val { Plain = 8 }, new Val { Array = new [] { 2m, 6m } } };
            cfg.Dictionary = new Dictionary<string, Val>() { { "demo", new Val { Plain = 14, Array = new [] { 3m, 15m } } } };
            cfg.Nested = new Val { Plain = 4, Array = new [] { 4m, 13m } };

            string yamlFile = null;
            try
            {
                yamlFile = Path.GetTempFileName();
                var serializer = new SerializerBuilder().Build();
                File.WriteAllText(yamlFile, serializer.Serialize(cfg));

                var builder = new ConfigurationBuilder();
                builder.AddYamlFile(yamlFile);

                // act
                var result = builder.Build<Config>();

                // assert
                result.IsNotDefault();
                result.Flag.IsTrue();
                result.Plain.IsEqual(7);
                result.Array.SequenceEqual(new [] { 4, 7 }).IsTrue();
                result.Matrix.Has(2);
                result.Matrix.At(0).SequenceEqual(new [] { 3, 2 }).IsTrue();
                result.Matrix.At(1).SequenceEqual(new [] { 5, 4 }).IsTrue();
                result.List.Has(2);
                result.List[0].Plain.IsEqual(8);
                result.List[0].Array.IsDefault();
                result.List[1].Plain.IsEqual(0);
                result.List[1].Array.SequenceEqual(new [] { 2m, 6m }).IsTrue();
                IDictionary<string, Val> dict = result.Dictionary;
                dict.Has(1);
                dict.At("demo").Plain.IsEqual(14);
                dict.At("demo").Array.SequenceEqual(new [] { 3m, 15m }).IsTrue();
                result.Nested.Plain.IsEqual(4);
                result.Nested.Array.SequenceEqual(new [] { 4m, 13m }).IsTrue();
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(yamlFile) && File.Exists(yamlFile))
                    File.Delete(yamlFile);
            }
        }
    }
}