using Annium.Localization.Abstractions;
using Annium.Localization.Yaml;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class LocalizationOptionsExtensions
    {
        public static LocalizationOptions UseYamlStorage(
            this LocalizationOptions options
        )
        {
            options.SetLocaleStorage(services =>
            {
                services.AddSingleton<ILocaleStorage, Storage>();
            });

            return options;
        }
    }
}