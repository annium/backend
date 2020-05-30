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
            var location = Path.GetDirectoryName(assembly.Location);

            var assemblyNamePath = Path.Combine(assembly.GetName().Name!.Split('.'));
            var targetNamespacePath = Path.Combine(target.Namespace?.Split('.') ?? Array.Empty<string>());
            var localeRelativePath = Path.GetRelativePath(assemblyNamePath, targetNamespacePath);

            var file = Path.Combine(location ?? string.Empty, localeRelativePath, "locale", $"{culture.TwoLetterISOLanguageName}.yml");

            return ResolveLocale(file);
        }

        private IReadOnlyDictionary<string, string> ResolveLocale(string file)
        {
            lock (locales)
            {
                if (locales.TryGetValue(file, out var locale))
                    return locale;

                locale = File.Exists(file)
                    ? deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file))
                    : new Dictionary<string, string>();

                locales[file] = locale;

                return locale;
            }
        }
    }
}