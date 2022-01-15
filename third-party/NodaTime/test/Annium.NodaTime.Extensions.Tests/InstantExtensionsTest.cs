using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class InstantExtensionsTest
{
    [Fact]
    public void FromUnixTimeMinutes()
    {
        // arrange
        var value = InstantExtensions.FromUnixTimeMinutes(1_000_000L);

        // assert
        value.Is(new LocalDateTime(1971, 11, 26, 10, 40).InUtc().ToInstant());
    }

    [Fact]
    public void ToUnixTimeMinutes()
    {
        // arrange
        var value = new LocalDateTime(1971, 11, 26, 10, 40).InUtc().ToInstant().ToUnixTimeMinutes();

        // assert
        value.Is(1_000_000L);
    }

    [Fact]
    public void FloorToSecond()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).FloorToSecond();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(99999));
    }

    [Fact]
    public void FloorToMinute()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).FloorToMinute();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(99960));
    }

    [Fact]
    public void FloorToHour()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).FloorToHour();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(97200));
    }

    [Fact]
    public void FloorToDay()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).FloorToDay();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(86400));
    }

    [Fact]
    public void CeilToSecond()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).CeilToSecond();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(100000));
    }

    [Fact]
    public void CeilToMinute()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).CeilToMinute();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(100020));
    }

    [Fact]
    public void CeilToHour()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).CeilToHour();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(100800));
    }

    [Fact]
    public void CeilToDay()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).CeilToDay();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(172800));
    }
}