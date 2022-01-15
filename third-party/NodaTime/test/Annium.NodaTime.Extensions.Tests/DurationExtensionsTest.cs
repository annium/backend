using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class DurationExtensionsTest
{
    [Fact]
    public void FloorToSecond()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).FloorToSecond();

        // assert
        value.Is(Duration.FromSeconds(99999));
    }

    [Fact]
    public void FloorToMinute()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).FloorToMinute();

        // assert
        value.Is(Duration.FromSeconds(99960));
    }

    [Fact]
    public void FloorToHour()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).FloorToHour();

        // assert
        value.Is(Duration.FromSeconds(97200));
    }

    [Fact]
    public void FloorToDay()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).FloorToDay();

        // assert
        value.Is(Duration.FromSeconds(86400));
    }

    [Fact]
    public void CeilToSecond()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).CeilToSecond();

        // assert
        value.Is(Duration.FromSeconds(100000));
    }

    [Fact]
    public void CeilToMinute()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).CeilToMinute();

        // assert
        value.Is(Duration.FromSeconds(100020));
    }

    [Fact]
    public void CeilToHour()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).CeilToHour();

        // assert
        value.Is(Duration.FromSeconds(100800));
    }

    [Fact]
    public void CeilToDay()
    {
        // arrange
        var value = Duration.FromTicks(999_999_999_999L).CeilToDay();

        // assert
        value.Is(Duration.FromSeconds(172800));
    }
}