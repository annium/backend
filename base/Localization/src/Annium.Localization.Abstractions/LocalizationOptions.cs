using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Localization.Abstractions
{
    public class LocalizationOptions
    {
        internal IServiceCollection LocaleStorageServices { get; private set; }
        internal Func<CultureInfo> CultureAccessor { get; private set; } = () => CultureInfo.CurrentCulture;

        internal LocalizationOptions() { }

        public LocalizationOptions SetLocaleStorage(Action<IServiceCollection> configure)
        {
            configure(LocaleStorageServices = new ServiceCollection());

            return this;
        }

        public LocalizationOptions UseCulture(CultureInfo culture)
        {
            CultureAccessor = () => culture;

            return this;
        }

        public LocalizationOptions UseCulture(Func<CultureInfo> accessor)
        {
            CultureAccessor = accessor;

            return this;
        }
    }
}