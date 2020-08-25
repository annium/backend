using System;
using System.Linq;
using Annium.Configuration.CommandLine;
using Annium.Configuration.CommandLine.Internal;

namespace Annium.Configuration.Abstractions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddCommandLineArgs(
            this IConfigurationBuilder builder
        )
        {
            return AddCommandLineArgs(builder, Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        public static IConfigurationBuilder AddCommandLineArgs(
            this IConfigurationBuilder builder,
            string[] args
        )
        {
            var configuration = new CommandLineConfigurationProvider(args).Read();

            return builder.Add(configuration);
        }
    }
}