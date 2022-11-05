using System.Collections.Generic;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests;
using Annium.Core.DependencyInjection;
using Annium.Data.Models.Extensions;
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
        result.Array.SequenceEqual(new[] { 4, 7 }).IsTrue();
        result.Nested.Plain.IsEqual(4);
        result.Nested.Array.SequenceEqual(new[] { 4m, 13m }).IsTrue();
        result.Nested.IsEqual(nested);
        nested.IsShallowEqual(new Val { Plain = 4, Array = new[] { 4m, 13m } });
    }
}

internal static class ListExtensions
{
    public static void AddRange<T>(this List<T> list, params T[] values)
    {
        list.AddRange(values);
    }
}