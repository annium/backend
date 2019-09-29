using System.Collections.Generic;
using System.Globalization;
using Annium.Localization.Abstractions;
using Annium.Localization.InMemoryStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class LocalizationOptionsExtensions
    {
        public static LocalizationOptions UseInMemoryStorage(
            this LocalizationOptions options
        )
        {
            return options.UseInMemoryStorage(new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>());
        }

        public static LocalizationOptions UseInMemoryStorage(
            this LocalizationOptions options,
            IReadOnlyDictionary<CultureInfo, IReadOnlyDictionary<string, string>> locales
        )
        {
            options.SetLocaleStorage(services =>
            {
                var storage = new Storage(locales ?? new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>());
                services.AddSingleton<ILocaleStorage>(storage);
            });

            return options;
        }
    }
}