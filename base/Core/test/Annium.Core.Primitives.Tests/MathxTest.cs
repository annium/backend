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
            var b = 9f;
            var c = 0f;
            var d = 10f;

            // assert
            Mathx.RelativeDiff(a, b).IsDefault();
            Mathx.RelativeDiff(a, c).Is(float.PositiveInfinity);
            Mathx.RelativeDiff(a, d).Is(0.1f);
        }

        [Fact]
        public void Double()
        {
            // arrange
            var a = 9d;
            var b = 9d;
            var c = 0d;
            var d = 10d;

            // assert
            Mathx.RelativeDiff(a, b).IsDefault();
            Mathx.RelativeDiff(a, c).Is(float.PositiveInfinity);
            Mathx.RelativeDiff(a, d).Is(0.1d);
        }

        [Fact]
        public void Decimal()
        {
            // arrange
            var a = 0.99m;
            var b = 0.99m;
            var c = 0m;
            var d = 1m;

            // assert
            Mathx.RelativeDiff(a, b).IsDefault();
            Mathx.RelativeDiff(a, c).Is(System.Decimal.MaxValue);
            Mathx.RelativeDiff(a, d).Is(0.01m);
        }
    }
}