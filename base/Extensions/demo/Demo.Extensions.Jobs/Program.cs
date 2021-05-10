using System;
using System.Threading;
using Annium.Core.DependencyInjection;
using Annium.Core.Entrypoint;
using Annium.Extensions.Jobs;
using NodaTime;
using Interval = Annium.Extensions.Jobs.Interval;

namespace Demo.Extensions.Jobs
{
    public class Program
    {
        private static void Run(
            IServiceProvider provider,
            string[] args,
            CancellationToken ct
        )
        {
            var resolver = provider.Resolve<IIntervalResolver>();

            var isMatch = resolver.GetMatcher(Interval.Minutely);

            var matched = isMatch(GetInstant(2000, 1, 12, 5, 6, 15));
        }

        public static int Main(string[] args) => new Entrypoint()
            .UseServicePack<ServicePack>()
            .Run(Run, args);

        private static Instant GetInstant(int year, int month, int day, int hour, int minute, int second) =>
            Instant.FromDateTimeUtc(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc));
    }
}