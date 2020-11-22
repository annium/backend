using System;
using System.Collections.Generic;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Testing;
using Xunit;

namespace Annium.Localization.Abstractions.Tests
{
    public class LocalizerTest
    {
        [Fact]
        public void Localization_Base_Works()
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
        public void Localization_WithParams_Works()
        {
            // arrange
            var localizer = GetLocalizer(opts => { });

            // act
            var iv = localizer["test params", 5];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
            var en = localizer["test params", 5];
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
            var ru = localizer["test params", 5];

            // assert
            iv.IsEqual("test params");
            en.IsEqual("demo 5");
            ru.IsEqual("демо 5");
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
            var container = new ServiceContainer();

            var locales = new Dictionary<CultureInfo, IReadOnlyDictionary<string, string>>();
            locales[CultureInfo.GetCultureInfo("en")] = new Dictionary<string, string> { { "test", "demo" }, { "test params", "demo {0}" } };
            locales[CultureInfo.GetCultureInfo("ru")] = new Dictionary<string, string> { { "test", "демо" }, { "test params", "демо {0}" } };

            container.AddLocalization(opts => configure(opts.UseInMemoryStorage(locales)));

            return container.BuildServiceProvider().Resolve<ILocalizer<LocalizerTest>>();
        }
    }
}