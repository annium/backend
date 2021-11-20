using Annium.Testing;
using NodaTime;
using Xunit;

namespace Annium.NodaTime.Extensions.Tests;

public class LocalDateTimeExtensionsTest
{
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
}