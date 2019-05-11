using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Testing;
using Newtonsoft.Json;

namespace Annium.Extensions.Configuration.Tests
{
    public class JsonConfigurationProviderTest
    {
        [Fact]
        public void JsonConfiguration_Works()
        {
            // arrange
            var cfg = new Config();
            cfg.Flag = true;
            cfg.Plain = 7;
            cfg.Array = new [] { 4, 7 };
            cfg.List = new List<Val>() { new Val { Plain = 8 }, new Val { Array = new [] { 2m, 6m } } };
            cfg.Dictionary = new Dictionary<string, Val>() { { "demo", new Val { Plain = 14, Array = new [] { 3m, 15m } } } };
            cfg.Nested = new Val { Plain = 4, Array = new [] { 4m, 13m } };

            string jsonFile = null;
            try
            {
                jsonFile = Path.GetTempFileName();
                File.WriteAllText(jsonFile, JsonConvert.SerializeObject(cfg));

                var builder = new ConfigurationBuilder();
                builder.AddJsonFile(jsonFile);

                // act
                var result = builder.Build<Config>();

                // assert
                result.IsNotDefault();
                result.Flag.IsTrue();
                result.Plain.IsEqual(7);
                result.Array.SequenceEqual(new [] { 4, 7 }).IsTrue();
                result.List.Has(2);
                result.List[0].Plain.IsEqual(8);
                result.List[0].Array.IsEmpty();
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
                if (!string.IsNullOrWhiteSpace(jsonFile) && File.Exists(jsonFile))
                    File.Delete(jsonFile);
            }
        }
    }
}