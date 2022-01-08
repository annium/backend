using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class LocalDateTimeExtensionsTest
{
    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(new LocalDateTime(1971, 11, 26, 10, 40));
    }

    [Fact]
    public void FromUnixTimeSeconds()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeSeconds(60_000_000L);

        // assert
        value.Is(new LocalDateTime(1971, 11, 26, 10, 40));
    }

    [Fact]
    public void FromUnixTimeMilliseconds()
    {
        // arrange
        var value = LocalDateTimeExtensions.FromUnixTimeMilliseconds(60_000_000_000L);

        // assert
        value.Is(new LocalDateTime(1971, 11, 26, 10, 40));
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
        var value = new LocalDateTime(1971, 11, 26, 10, 40).ToUnixTimeMinutes();

        // assert
        value.Is(1_000_000L);
    }

    [Fact]
    public void ToUnixTimeSeconds()
    {
        // arrange
        var value = new LocalDateTime(1971, 11, 26, 10, 40).ToUnixTimeSeconds();

        // assert
        value.Is(60_000_000L);
    }

    [Fact]
    public void ToUnixTimeMilliseconds()
    {
        // arrange
        var value = new LocalDateTime(1971, 11, 26, 10, 40).ToUnixTimeMilliseconds();

        // assert
        value.Is(60_000_000_000L);
    }
}