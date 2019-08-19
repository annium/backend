using System.Collections.Generic;
using System.Linq;
using Annium.Configuration.Abstractions;
using Annium.Configuration.Tests;
using Annium.Testing;

namespace Annium.Configuration.CommandLine.Tests
{
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

            var builder = new ConfigurationBuilder();
            builder.AddCommandLineArgs(args.ToArray());

            // act
            var result = builder.Build<Config>();

            // assert
            result.IsNotDefault();
            result.Flag.IsTrue();
            result.Plain.IsEqual(7);
            result.Array.SequenceEqual(new [] { 4, 7 }).IsTrue();
            result.Nested.Plain.IsEqual(4);
            result.Nested.Array.SequenceEqual(new [] { 4m, 13m }).IsTrue();
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