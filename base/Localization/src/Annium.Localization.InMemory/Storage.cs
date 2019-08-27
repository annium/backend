using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Localization.Abstractions;

namespace Annium.Localization.InMemoryStorage
{
    internal class Storage : ILocaleStorage
    {
        private readonly IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales;

        public Storage(
            IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales
        )
        {
            this.locales = locales;
        }

        public IReadOnlyDictionary<string, string> LoadLocale(Type target, CultureInfo culture)
        {
            return locales.TryGetValue(culture, out var locale) ? locale : new Dictionary<string, string>();
        }
    }
}