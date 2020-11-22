using System;
using System.Globalization;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Localization.Abstractions;

namespace Demo.Localization
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var localizer = provider.Resolve<ILocalizer<Program>>();

            var entry = "demo";
            Console.WriteLine($"Source: {entry}");
            Console.WriteLine($"IV:     {localizer[entry]}");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
            Console.WriteLine($"EN:     {localizer[entry]}");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru");
            Console.WriteLine($"RU:     {localizer[entry]}");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Console.WriteLine($"IV:     {localizer[entry]}");
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);
    }
}