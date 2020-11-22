using System;
using Annium.Localization.Abstractions;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddLocalization(
            this IServiceContainer container,
            Action<LocalizationOptions> configure
        )
        {
            var options = new LocalizationOptions();
            configure(options);

            foreach (var service in options.LocaleStorageServices)
                container.Add(service);

            container.Add(options.CultureAccessor).Singleton();

            container.Add(typeof(Localizer<>)).As(typeof(ILocalizer<>)).Singleton();

            return container;
        }
    }
}