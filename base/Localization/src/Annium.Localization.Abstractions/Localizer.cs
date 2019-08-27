using System;
using System.Collections.Generic;
using System.Globalization;

namespace Annium.Localization.Abstractions
{
    internal class Localizer<T> : ILocalizer<T>
    {
        private readonly IDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales =
        new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
        private readonly ILocaleStorage storage;
        private readonly Func<CultureInfo> getCulture;

        public Localizer(
            ILocaleStorage storage,
            Func<CultureInfo> getCulture
        )
        {
            this.storage = storage;
            this.getCulture = getCulture;
        }

        public string this[string entry]
        {
            get
            {
                var locale = ResolveLocale();

                return locale.TryGetValue(entry, out var translation) ? translation : entry;
            }
        }

        private IReadOnlyDictionary<string, string> ResolveLocale()
        {
            var culture = getCulture();
            lock(locales)
            {
                if (locales.TryGetValue(culture, out var locale))
                    return locale;

                return locales[culture] = storage.LoadLocale(typeof(T), culture);
            }
        }
    }
}