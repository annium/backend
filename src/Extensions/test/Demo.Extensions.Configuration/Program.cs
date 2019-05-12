using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Annium.Extensions.Configuration;
using Annium.Extensions.Entrypoint;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Demo.Extensions.Configuration
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            // TestCli();
            // TestJson();
            TestYaml();
        }

        private static void TestCli()
        {
            // arrange
            var args = new List<string>();
            args.AddRange("-plain", "7");
            args.AddRange("-array", "4", "-array", "7");
            args.AddRange("-list.plain", "8");
            args.AddRange("-list.array", "2", "-list.array", "6");

            var builder = new ConfigurationBuilder();
            builder.AddCommandLineArgs(args.ToArray());

            // act
            var result = builder.Build<Config>();
        }

        private static void TestJson()
        {
            var cfg = new Config();
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
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(jsonFile) && File.Exists(jsonFile))
                    File.Delete(jsonFile);
            }
        }

        private static void TestYaml()
        {
            var cfg = new Config();
            cfg.Plain = 7;
            cfg.Array = new [] { 4, 7 };
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
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(yamlFile) && File.Exists(yamlFile))
                    File.Delete(yamlFile);
            }
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);

        private class Config
        {
            public int Plain { get; set; }

            public int[] Array { get; set; }

            public List<Val> List { get; set; }

            public Dictionary<string, Val> Dictionary { get; set; }

            public Val Nested { get; set; }
        }

        private class Val
        {
            public int Plain { get; set; }

            public decimal[] Array { get; set; }
        }
    }

    internal static class ListExtensions
    {
        public static void AddRange<T>(this List<T> list, params T[] values)
        {
            list.AddRange(values);
        }
    }
}