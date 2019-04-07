using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Localization;
using YamlDotNet.Serialization;

namespace Annium.Extensions.Localization
{
    public class YamlStringLocalizer : IStringLocalizer
    {
        private static readonly IDeserializer deserializer = new DeserializerBuilder().Build();

        private readonly string root;

        private readonly IDictionary<CultureInfo, IReadOnlyDictionary<string, string>> translations =
            new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();

        public YamlStringLocalizer(string root)
        {
            this.root = root;
        }

        public LocalizedString this[string name] =>
            new LocalizedString(name, GetTranslation(name) ?? name);

        public LocalizedString this[string name, params object[] arguments] =>
            new LocalizedString(name, string.Format(GetTranslation(name) ?? name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            throw new NotImplementedException();

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        private string GetTranslation(string key)
        {
            var culture = CultureInfo.CurrentCulture;

            lock(translations)
            {
                if (!translations.ContainsKey(culture))
                {
                    var file = Path.Combine(root, $"{culture.TwoLetterISOLanguageName}.yml");

                    if (File.Exists(file))
                        translations[culture] = deserializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file));
                    else
                        translations[culture] = new Dictionary<string, string>();
                }
            }

            var cultureTranslations = translations[culture];

            return cultureTranslations.ContainsKey(key) ? cultureTranslations[key] : null;
        }
    }
}