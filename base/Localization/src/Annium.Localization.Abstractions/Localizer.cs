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

        public string this[string entry] => Translate(entry);

        public string this[string entry, params object[] arguments] => string.Format(getCulture(), Translate(entry), arguments);

        public string this[string entry, IEnumerable<object> arguments] => string.Format(getCulture(), Translate(entry), arguments);

        private string Translate(string entry)
        {
            var culture = getCulture();
            var locale = ResolveLocale(culture);

            return locale.TryGetValue(entry, out var translation) ? translation : entry;
        }

        private IReadOnlyDictionary<string, string> ResolveLocale(CultureInfo culture)
        {
            lock(locales)
            {
                if (locales.TryGetValue(culture, out var locale))
                    return locale;

                return locales[culture] = storage.LoadLocale(typeof(T), culture);
            }
        }
    }
}