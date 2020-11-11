using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Localization.Abstractions;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Annium.Localization.InMemory.Tests
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

            var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
            locales[CultureInfo.GetCultureInfo("en")] = new Dictionary<string, string> { { "test", "demo" } };
            locales[CultureInfo.GetCultureInfo("ru")] = new Dictionary<string, string> { { "test", "демо" } };

            services.AddLocalization(opts => opts.UseInMemoryStorage(locales));

            return services.BuildServiceProvider().GetRequiredService<ILocalizer<StorageTest>>();
        }
    }
}