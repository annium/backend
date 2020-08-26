using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Annium.Configuration.Json.Internal;

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

            var raw = File.ReadAllText(path);
            var configuration = new JsonConfigurationProvider(raw).Read();

            return builder.Add(configuration);
        }

        public static async Task<IConfigurationBuilder> AddRemoteJson(
            this IConfigurationBuilder builder,
            string uri,
            bool optional = false
        )
        {
            var client = new HttpClient();
            var message = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await client.SendAsync(message);
            if (!response.IsSuccessStatusCode)
                if (optional)
                    return builder;
                else
                    throw new FileNotFoundException($"Json configuration not available at {uri} and is not optional");

            var raw = await response.Content.ReadAsStringAsync();
            var configuration = new JsonConfigurationProvider(raw).Read();

            return builder.Add(configuration);
        }
    }
}