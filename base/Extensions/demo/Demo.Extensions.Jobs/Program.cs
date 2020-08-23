using System;
using System.Threading;
using Annium.Core.Entrypoint;
using Annium.Extensions.Jobs;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Demo.Extensions.Jobs
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken token
        )
        {
            var resolver = provider.GetRequiredService<IIntervalResolver>();

            var isMatch = resolver.GetMatcher(Annium.Extensions.Jobs.Interval.Minutely);

            var matched = isMatch(GetInstant(2000, 1, 12, 5, 6, 15));
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);

        private static Instant GetInstant(int year, int month, int day, int hour, int minute, int second) =>
            Instant.FromDateTimeUtc(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc));
    }
}