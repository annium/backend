using System;
using System.Linq;
using Annium.Configuration.CommandLine.Internal;

namespace Annium.Configuration.Abstractions
{
    public static class ConfigurationContainerExtensions
    {
        public static IConfigurationContainer AddCommandLineArgs(
            this IConfigurationContainer container
        )
        {
            return AddCommandLineArgs(container, Environment.GetCommandLineArgs().Skip(1).ToArray());
        }

        public static IConfigurationContainer AddCommandLineArgs(
            this IConfigurationContainer container,
            string[] args
        )
        {
            var configuration = new CommandLineConfigurationProvider(args).Read();

            return container.Add(configuration);
        }
    }
}