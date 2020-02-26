using System.IO;
using Annium.Configuration.Json;

namespace Annium.Configuration.Abstractions
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddJsonFile(
            this IConfigurationBuilder builder,
            string path,
            bool optional = false
        )
        {
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
                if (optional)
                    return builder;
                else
                    throw new FileNotFoundException($"Json configuration file {path} not found and is not optional");

            var configuration = new JsonConfigurationProvider(path).Read();

            return builder.Add(configuration);
        }
    }
}