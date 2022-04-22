using System.Collections.Generic;
using System.IO;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests;
using Annium.Core.DependencyInjection;
using Annium.Serialization.Abstractions;
using YamlDotNet.Serialization;

// TestBuilder();
// TestCli();
TestJson();
// TestYaml();

void TestBuilder()
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

void TestCli()
{
    // arrange
    var args = new List<string>();
    args.AddRange("-plain", "7");
    args.AddRange("-array", "4", "-array", "7");
    args.AddRange("-list.plain", "8");
    args.AddRange("-list.array", "2", "-list.array", "6");

    Helper.GetProvider<Config>(builder => builder.AddCommandLineArgs(args.ToArray()));
}

void TestJson()
{
    var cfg = new Config
    {
        Flag = true,
        Plain = 7,
        Array = new[] { 4, 7 },
        Matrix = new List<int[]> { new[] { 3, 2 }, new[] { 5, 4 } },
        List = new List<Val> { new() { Plain = 8 }, new() { Array = new[] { 2m, 6m } } },
        Dictionary = new Dictionary<string, Val> { { "demo", new Val { Plain = 14, Array = new[] { 3m, 15m } } } },
        Nested = new Val { Plain = 4, Array = new[] { 4m, 13m } },
        Abstract = new ConfigTwo { Value = 10 },
    };

    string jsonFile = string.Empty;
    try
    {
        jsonFile = Path.GetTempFileName();
        var container = new ServiceContainer();
        container.AddRuntimeTools(typeof(Program).Assembly, false);
        container.AddJsonSerializers().SetDefault();
        var serializer = container
            .BuildServiceProvider()
            .Resolve<ISerializer<string>>();
        File.WriteAllText(jsonFile, serializer.Serialize(cfg));

        Helper.GetProvider<Config>(builder => builder.AddJsonFile(jsonFile)).Resolve<Config>();
    }
    finally
    {
        if (!string.IsNullOrWhiteSpace(jsonFile) && File.Exists(jsonFile))
            File.Delete(jsonFile);
    }
}

void TestYaml()
{
    var cfg = new Config
    {
        Flag = true,
        Plain = 7,
        Array = new[] { 4, 7 },
        Matrix = new List<int[]> { new[] { 3, 2 }, new[] { 5, 4 } },
        List = new List<Val> { new() { Plain = 8 }, new() { Array = new[] { 2m, 6m } } },
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


internal static class ListExtensions
{
    public static void AddRange<T>(this List<T> list, params T[] values)
    {
        list.AddRange(values);
    }
}