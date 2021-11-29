using Annium.Testing;
using Xunit;

namespace Annium.Core.Primitives.Tests
{
    public class MathxTest
    {
        [Fact]
        public void Float()
        {
            // arrange
            var a = 9f;
            var b = -9f;
            var c = 0f;
            var d = 10f;
            var e = -10f;

            // assert
            Mathx.RelativeDiff(a, a).IsDefault();
            Mathx.RelativeDiff(a, b).Is(2f);
            Mathx.RelativeDiff(a, c).Is(float.PositiveInfinity);
            Mathx.RelativeDiff(a, d).Is(0.1f);
            Mathx.RelativeDiff(a, e).Is(1.9f);
        }

        [Fact]
        public void Double()
        {
            // arrange
            var a = 9d;
            var b = -9d;
            var c = 0d;
            var d = 10d;
            var e = -10d;

            // assert
            Mathx.RelativeDiff(a, a).IsDefault();
            Mathx.RelativeDiff(a, b).Is(2d);
            Mathx.RelativeDiff(a, c).Is(float.PositiveInfinity);
            Mathx.RelativeDiff(a, d).Is(0.1d);
            Mathx.RelativeDiff(a, e).Is(1.9d);
        }

        [Fact]
        public void Decimal()
        {
            // arrange
            var a = 0.99m;
            var b = -0.99m;
            var c = 0m;
            var d = 1m;
            var e = -1m;

            // assert
            Mathx.RelativeDiff(a, a).IsDefault();
            Mathx.RelativeDiff(a, b).Is(2m);
            Mathx.RelativeDiff(a, c).Is(System.Decimal.MaxValue);
            Mathx.RelativeDiff(a, d).Is(0.01m);
            Mathx.RelativeDiff(a, e).Is(1.99m);
        }
    }
}