using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class DurationExtensionsTest
{
    [Fact]
    public void AlignToSecond()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).AlignToSecond();

        // assert
        value.Is(Duration.FromSeconds(99999));
    }

    [Fact]
    public void AlignToMinute()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).AlignToMinute();

        // assert
        value.Is(Duration.FromSeconds(99960));
    }

    [Fact]
    public void AlignToHour()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).AlignToHour();

        // assert
        value.Is(Duration.FromSeconds(97200));
    }

    [Fact]
    public void AlignToDay()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).AlignToDay();

        // assert
        value.Is(Duration.FromSeconds(86400));
    }
}