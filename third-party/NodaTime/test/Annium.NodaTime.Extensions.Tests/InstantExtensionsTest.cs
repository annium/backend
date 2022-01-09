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
    public void AlignToSecond()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).AlignToSecond();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(99999));
    }

    [Fact]
    public void AlignToMinute()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).AlignToMinute();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(99960));
    }

    [Fact]
    public void AlignToHour()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).AlignToHour();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(97200));
    }

    [Fact]
    public void AlignToDay()
    {
        // arrange
        var value = Instant.MinValue + Duration.FromTicks(999_999_999_999L).AlignToDay();

        // assert
        value.Is(Instant.MinValue + Duration.FromSeconds(86400));
    }
}