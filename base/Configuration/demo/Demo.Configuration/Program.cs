using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Serialization.Abstractions;
using Demo.Extensions.Configuration;
using YamlDotNet.Serialization;

namespace Demo.Configuration
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            // TestBuilder();
            // TestCli();
            TestJson();
            // TestYaml();
        }

        private static void TestBuilder()
        {
            // arrange
            var cfg = new Dictionary<string[], string>
            {
                [new[] { "plain" }] = "10",
                [new[] { "abstract_config", "type" }] = "ConfigOne",
                [new[] { "abstract_config", "value" }] = "14"
            };
            Helper.GetProvider<Config>(builder => builder.Add(cfg)).Resolve<Config>();
        }

        private static void TestCli()
        {
            // arrange
            var args = new List<string>();
            args.AddRange("-plain", "7");
            args.AddRange("-array", "4", "-array", "7");
            args.AddRange("-list.plain", "8");
            args.AddRange("-list.array", "2", "-list.array", "6");

            Helper.GetProvider<Config>(builder => builder.AddCommandLineArgs(args.ToArray()));
        }

        private static void TestJson()
        {
            var cfg = new Config
            {
                Flag = true,
                Plain = 7,
                Array = new[] { 4, 7 },
                Matrix = new List<int[]> { new[] { 3, 2 }, new[] { 5, 4 } },
                List = new List<Val> { new Val { Plain = 8 }, new Val { Array = new[] { 2m, 6m } } },
                Dictionary = new Dictionary<string, Val> { { "demo", new Val { Plain = 14, Array = new[] { 3m, 15m } } } },
                Nested = new Val { Plain = 4, Array = new[] { 4m, 13m } },
                Abstract = new ConfigTwo { Value = 10 },
            };

            string jsonFile = string.Empty;
            try
            {
                jsonFile = Path.GetTempFileName();
                var serializer = new ServiceContainer()
                    .AddRuntimeTools(typeof(Program).Assembly, false)
                    .AddJsonSerializers()
                    .BuildServiceProvider()
                    .Resolve<IIndex<string, ISerializer<string>>>()
                    [MediaTypeNames.Application.Json];
                File.WriteAllText(jsonFile, serializer.Serialize(cfg));

                Helper.GetProvider<Config>(builder => builder.AddJsonFile(jsonFile)).Resolve<Config>();
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(jsonFile) && File.Exists(jsonFile))
                    File.Delete(jsonFile);
            }
        }

        private static void TestYaml()
        {
            var cfg = new Config
            {
                Flag = true,
                Plain = 7,
                Array = new[] { 4, 7 },
                Matrix = new List<int[]> { new[] { 3, 2 }, new[] { 5, 4 } },
                List = new List<Val> { new Val { Plain = 8 }, new Val { Array = new[] { 2m, 6m } } },
                Dictionary = new Dictionary<string, Val> { { "demo", new Val { Plain = 14, Array = new[] { 3m, 15m } } } },
                Nested = new Val { Plain = 4, Array = new[] { 4m, 13m } }
            };

            string yamlFile = string.Empty;
            try
            {
                yamlFile = Path.GetTempFileName();
                var serializer = new SerializerBuilder().Build();
                File.WriteAllText(yamlFile, serializer.Serialize(cfg));

                Helper.GetProvider<Config>(builder => builder.AddYamlFile(yamlFile)).Resolve<Config>();
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
    }

    internal static class ListExtensions
    {
        public static void AddRange<T>(this List<T> list, params T[] values)
        {
            list.AddRange(values);
        }
    }
}