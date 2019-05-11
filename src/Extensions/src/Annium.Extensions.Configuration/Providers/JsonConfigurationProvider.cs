using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Annium.Extensions.Configuration
{
    internal class JsonConfigurationProvider : IConfigurationProvider
    {
        private readonly string filePath;

        private Dictionary<string, string> data = new Dictionary<string, string>();

        private Stack<string> context = new Stack<string>();

        private string path => string.Join(ConfigurationBuilder.Separator, context.Reverse());

        public JsonConfigurationProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public IReadOnlyDictionary<string, string> Read()
        {
            var token = JObject.Parse(File.ReadAllText(filePath));

            Process(token);

            return data;
        }

        private void Process(JObject token)
        {
            foreach (var property in token.Properties())
            {
                context.Push(property.Name);

                if (property.Value is JObject obj)
                    Process(obj);
                else if (property.Value is JArray arr)
                    Process(arr);
                else
                    Process(property.Value);

                context.Pop();
            }
        }

        private void Process(JArray token)
        {
            var index = 0;
            foreach (var item in token)
            {
                context.Push(index.ToString());

                if (item is JArray arr)
                    Process(arr);
                else if (item is JObject obj)
                    Process(obj);
                else
                    Process(item);

                context.Pop();
                index++;
            }
        }

        private void Process(JToken token)
        {
            data[path] = token.ToString();
        }
    }

    public static class JsonConfigurationProviderExtensions
    {
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder builder, string path, bool optional = false)
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