using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Localization.Abstractions.Tests
{
    public class LocalizerTest
    {
        [Fact]
        public void Localization_Works()
        {
            // arrange
            var localizer = GetLocalizer(opts => { });

            // act
            var iv = localizer["test"];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
            var en = localizer["test"];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
            var ru = localizer["test"];

            // assert
            iv.IsEqual("test");
            en.IsEqual("demo");
            ru.IsEqual("демо");
        }

        [Fact]
        public void Localization_WithSpecifiedCulture_UsesSpecificCulture()
        {
            // arrange
            var localizer = GetLocalizer(opts => opts.UseCulture(CultureInfo.GetCultureInfo("en")));

            // act
            var iv = localizer["test"];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
            var en = localizer["test"];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
            var ru = localizer["test"];

            // assert
            iv.IsEqual("demo");
            en.IsEqual("demo");
            ru.IsEqual("demo");
        }

        [Fact]
        public void Localization_WithSpecifiedCultureAccessor_UsesCultureAccessor()
        {
            // arrange
            var localizer = GetLocalizer(opts => opts.UseCulture(() => CultureInfo.CurrentCulture));

            // act
            var iv = localizer["test"];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
            var en = localizer["test"];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
            var ru = localizer["test"];

            // assert
            iv.IsEqual("test");
            en.IsEqual("demo");
            ru.IsEqual("демо");
        }

        private ILocalizer<LocalizerTest> GetLocalizer(Action<LocalizationOptions> configure)
        {
            var services = new ServiceCollection();

            var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
            locales[CultureInfo.GetCultureInfo("en")] = new Dictionary<string, string>() { { "test", "demo" } };
            locales[CultureInfo.GetCultureInfo("ru")] = new Dictionary<string, string>() { { "test", "демо" } };

            services.AddLocalization(opts => configure(opts.UseInMemoryStorage(locales)));

            return services.BuildServiceProvider().GetRequiredService<ILocalizer<LocalizerTest>>();
        }
    }
}