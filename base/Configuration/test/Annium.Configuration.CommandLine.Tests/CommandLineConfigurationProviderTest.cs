using System.Collections.Generic;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Configuration.CommandLine.Tests;

public class CommandLineConfigurationProviderTest
{
    [Fact]
    public void CommandLineConfiguration_Works()
    {
        // arrange
        var args = new List<string>();
        args.AddRange("-flag");
        args.AddRange("-plain", "7");
        args.AddRange("-nullable", "3");
        args.AddRange("-array", "4", "-array", "7");
        args.AddRange("-nested.plain", "4");
        args.AddRange("-nested.array", "4", "-nested.array", "13");

        // act
        var provider = Helper.GetProvider<Config>(builder => builder.AddCommandLineArgs(args.ToArray()));
        var result = provider.Resolve<Config>();
        var nested = provider.Resolve<Val>();

        // assert
        result.IsNotDefault();
        result.Flag.IsTrue();
        result.Plain.Is(7);
        // result.Nullable.Is(3);
        result.Array.SequenceEqual(new[] { 4, 7 }).IsTrue();
        result.Nested.Plain.IsEqual(4);
        result.Nested.Array.SequenceEqual(new[] { 4m, 13m }).IsTrue();
        result.Nested.IsEqual(nested);
        nested.Plain.Is(4);
        nested.Array.SequenceEqual(new[] { 4m, 13m }).IsTrue();
    }
}

internal static class ListExtensions
{
    public static void AddRange<T>(this List<T> list, params T[] values)
    {
        list.AddRange(values);
    }
}