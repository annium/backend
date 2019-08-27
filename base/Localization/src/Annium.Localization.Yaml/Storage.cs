using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Annium.Localization.Abstractions;
using YamlDotNet.Serialization;

namespace Annium.Localization.Yaml
{
    internal class Storage : ILocaleStorage
    {
        private readonly IDeserializer deserializer = new DeserializerBuilder().Build();

        private readonly IDictionary<string, IReadOnlyDictionary<string, string>> locales =
            new Dictionary<string, IReadOnlyDictionary<string, string>>();

        public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
        {
            var assembly = target.GetTypeInfo().Assembly;
            var location = assembly.Location;
            var file = Path.Combine(
                Path.GetDirectoryName(location),
                Path.Combine(target.Namespace.Substring(assembly.GetName().Name.Length).Split('.')),
                "locale",
                $"{culture.TwoLetterISOLanguageName}.yml"
            );

            return ResolveLocale(file);
        }

        private IReadOnlyDictionary<string, string> ResolveLocale(string file)
        {
            lock(locales)
            {
                if (locales.TryGetValue(file, out var locale))
                    return locale;

                locale = File.Exists(file) ?
                    deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file)) :
                    new Dictionary<string, string>();

                locales[file] = locale;

                return locale;
            }
        }
    }
}