using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests;

public class DecimalExtensionsTest
{
    [Fact]
    public void DiffFrom()
    {
        // arrange
        var a = 0.99m;
        var b = -0.99m;
        var c = 0m;
        var d = 1m;
        var e = -1m;

        // assert
        a.DiffFrom(a).IsDefault();
        a.DiffFrom(b).Is(2m);
        a.DiffFrom(c).Is(System.Decimal.MaxValue);
        a.DiffFrom(d).Is(0.01m);
        a.DiffFrom(e).Is(1.99m);
    }

    [Fact]
    public void ToPretty()
    {
        // assert
        123.9214999m.ToPretty(0.01m).Is(124m);
        123.9214999m.ToPretty(0.1m).Is(120m);
        123.9214999m.ToPretty(0.5m).Is(100m);
    }
}