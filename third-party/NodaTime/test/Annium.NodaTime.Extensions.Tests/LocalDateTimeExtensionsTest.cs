using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class LocalDateTimeExtensionsTest
{
    private readonly LocalDateTime _moment = new(1971, 11, 26, 10, 40);

    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void FromUnixTimeSeconds()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeSeconds(60_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void FromUnixTimeMilliseconds()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeMilliseconds(60_000_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void GetYearMonth()
    {
        // arrange
        var value = _moment.GetYearMonth();

        // assert
        value.Is(new YearMonth(_moment.Era, _moment.YearOfEra, _moment.Month, _moment.Calendar));
    }

    [Fact]
    public void IsMidnight()
    {
        // arrange
        var midNight = new LocalDateTime(1, 2, 3, 0, 0, 0, 0);
        var nonMdNight = new LocalDateTime(1, 2, 3, 0, 0, 0, 1);

        // assert
        midNight.IsMidnight().IsTrue();
        nonMdNight.IsMidnight().IsFalse();
    }

    [Fact]
    public void ToUnixTimeMinutes()
    {
        // arrange
        var value = _moment.ToUnixTimeMinutes();

        // assert
        value.Is(1_000_000L);
    }

    [Fact]
    public void ToUnixTimeSeconds()
    {
        // arrange
        var value = _moment.ToUnixTimeSeconds();

        // assert
        value.Is(60_000_000L);
    }

    [Fact]
    public void ToUnixTimeMilliseconds()
    {
        // arrange
        var value = _moment.ToUnixTimeMilliseconds();

        // assert
        value.Is(60_000_000_000L);
    }
}