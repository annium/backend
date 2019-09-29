using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Annium.Configuration.Abstractions
{
    internal class JsonConfigurationProvider : ConfigurationProviderBase
    {
        private readonly string filePath;

        public JsonConfigurationProvider(string filePath)
        {
            this.filePath = filePath;
        }

        public override IReadOnlyDictionary<string[], string> Read()
        {
            Init();

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

                if (item is JObject obj)
                    Process(obj);
                else if (item is JArray arr)
                    Process(arr);
                else
                    Process(item);

                context.Pop();
                index++;
            }
        }

        private void Process(JToken token)
        {
            data[Path] = token.ToString();
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