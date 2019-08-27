using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Localization.Yaml.Tests
{
    public class StorageTest
    {
        [Fact]
        public void Localization_Works()
        {
            // arrange
            var localizer = GetLocalizer();

            // act
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
            var en = localizer["test"];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
            var ru = localizer["test"];

            // assert
            en.IsEqual("demo");
            ru.IsEqual("демо");
        }

        private ILocalizer<StorageTest> GetLocalizer()
        {
            var services = new ServiceCollection();

            services.AddLocalization(opts => opts.UseYamlStorage());

            return services.BuildServiceProvider().GetRequiredService<ILocalizer<StorageTest>>();
        }
    }
}