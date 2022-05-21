using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class ZonedDateTimeExtensionsTest
{
    private readonly ZonedDateTime _moment = new(new LocalDateTime(1971, 11, 26, 10, 40), DateTimeZone.Utc, Offset.Zero);

    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void FromUnixTimeSeconds()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeSeconds(60_000_000L);

        // assert
        value.Is(_moment);
    }

    [Fact]
    public void FromUnixTimeMilliseconds()
    {
        // arrange
        var value = ZonedDateTimeExtensions.FromUnixTimeMilliseconds(60_000_000_000L);

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
        var midNight = new ZonedDateTime(new LocalDateTime(1, 2, 3, 0, 0, 0, 0), DateTimeZone.Utc, Offset.Zero);
        var nonMdNight = new ZonedDateTime(new LocalDateTime(1, 2, 3, 0, 0, 0, 1), DateTimeZone.Utc, Offset.Zero);

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