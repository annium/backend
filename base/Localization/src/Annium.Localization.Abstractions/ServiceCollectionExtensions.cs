using System;
using Annium.Localization.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalization(
            this IServiceCollection services,
            Action<LocalizationOptions> configure
        )
        {
            var options = new LocalizationOptions();
            configure(options);

            foreach (var service in options.LocaleStorageServices)
                services.Add(service);

            services.AddSingleton(options.CultureAccessor);

            services.AddSingleton(typeof(ILocalizer<>), typeof(Localizer<>));

            return services;
        }
    }
}