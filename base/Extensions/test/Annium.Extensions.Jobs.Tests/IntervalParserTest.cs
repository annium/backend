using Annium.Core.DependencyInjection;
using Annium.Extensions.Jobs.Internal;
using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.Extensions.Jobs.Tests;

public class IntervalParserTest
{
    [Fact]
    public void Secondly_Works()
    {
        // arrange
        var parser = GetParser();

        // act - every second
        var resolver = parser.GetDelayResolver("* * * * *");

        // assert - every second
        resolver(GetDate(12, 5, 6, 15, 150)).Is(Ms(850));
        resolver(GetDate(12, 5, 6, 0, 0)).Is(Ms(0));
        resolver(GetDate(12, 5, 6, 0, 999)).Is(Ms(1));

        // act - every N second
        resolver = parser.GetDelayResolver("*/3 * * * *");

        // assert - every N second
        resolver(GetDate(12, 5, 6, 15, 150)).Is(Sec(2) + Ms(850));
        resolver(GetDate(12, 5, 6, 0, 0)).Is(Ms(0));
        resolver(GetDate(12, 5, 6, 0, 999)).Is(Sec(2) + Ms(1));
    }

    private static LocalDateTime GetDate(int day, int hour, int minute, int second, int ms) =>
        new(2000, 1, day, hour, minute, second, ms);

    private static Duration Day(int x) => Duration.FromDays(x);
    private static Duration Hour(int x) => Duration.FromHours(x);
    private static Duration Min(int x) => Duration.FromMinutes(x);
    private static Duration Sec(int x) => Duration.FromSeconds(x);
    private static Duration Ms(int x) => Duration.FromMilliseconds(x);

    private static IIntervalParser GetParser() => new ServiceContainer()
        .AddScheduler()
        .BuildServiceProvider()
        .Resolve<IIntervalParser>();
}