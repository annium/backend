using System.Collections.Generic;
using System.IO;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests;
using Annium.Core.DependencyInjection;
using Annium.Core.Runtime.Types;
using Annium.Serialization.Json;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.Json.Tests
{
    public class JsonConfigurationProviderTest
    {
        [Fact]
        public void JsonConfiguration_Works()
        {
            // arrange
            var cfg = new Config
            {
                Flag = true,
                Plain = 7,
                Array = new[] { 4, 7 },
                Matrix = new List<int[]>() { new[] { 3, 2 }, new[] { 5, 4 } },
                List = new List<Val>() { new Val { Plain = 8 }, new Val { Array = new[] { 2m, 6m } } },
                Dictionary = new Dictionary<string, Val>()
                    { { "demo", new Val { Plain = 14, Array = new[] { 3m, 15m } } } },
                Nested = new Val { Plain = 4, Array = new[] { 4m, 13m } },
                Abstract = new ConfigTwo { Value = 10 },
            };

            string jsonFile = string.Empty;
            try
            {
                jsonFile = Path.GetTempFileName();
                var typeManager = TypeManager.GetInstance(GetType().Assembly);
                var serializer = StringSerializer.Configure(opts => opts.ConfigureDefault(typeManager));
                File.WriteAllText(jsonFile, serializer.Serialize(cfg));

                // act
                var result = Helper.BuildConfiguration<Config>(builder => builder.AddJsonFile(jsonFile));

                // assert
                result.IsNotDefault();
                result.Flag.IsTrue();
                result.Plain.IsEqual(7);
                result.Array.SequenceEqual(new[] { 4, 7 }).IsTrue();
                result.Matrix.Has(2);
                result.Matrix.At(0).SequenceEqual(new[] { 3, 2 }).IsTrue();
                result.Matrix.At(1).SequenceEqual(new[] { 5, 4 }).IsTrue();
                result.List.Has(2);
                result.List[0].Plain.IsEqual(8);
                result.List[0].Array.IsEmpty();
                result.List[1].Plain.IsEqual(0);
                result.List[1].Array.SequenceEqual(new[] { 2m, 6m }).IsTrue();
                IDictionary<string, Val> dict = result.Dictionary;
                dict.Has(1);
                dict.At("demo").Plain.IsEqual(14);
                dict.At("demo").Array.SequenceEqual(new[] { 3m, 15m }).IsTrue();
                result.Nested.Plain.IsEqual(4);
                result.Nested.Array.SequenceEqual(new[] { 4m, 13m }).IsTrue();
                result.Abstract.As<ConfigTwo>().Value.IsEqual(10);
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(jsonFile) && File.Exists(jsonFile))
                    File.Delete(jsonFile);
            }
        }
    }
}