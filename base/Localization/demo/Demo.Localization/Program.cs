using System;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Localization.Abstractions;
using Demo.Localization;

await using var entry = Entrypoint.Default
    .UseServicePack<ServicePack>()
    .Setup();

var (provider, _) = entry;

var localizer = provider.Resolve<ILocalizer<Program>>();

var item = "demo";
Console.WriteLine($"Source: {item}");
Console.WriteLine($"IV:     {localizer[item]}");
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
Console.WriteLine($"EN:     {localizer[item]}");
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
Console.WriteLine($"RU:     {localizer[item]}");
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.WriteLine($"IV:     {localizer[item]}");