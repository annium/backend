using Annium.Testing;
using Xunit;

namespace Annium.Extensions.Primitives.Tests
{
    public class ArrayExtensionsTest
    {
        [Fact]
        public void Deconstruct_1_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, rest) = arr;
            x1.IsEqual(1);
            rest.IsEqual(new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public void Deconstruct_2_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, x2, rest) = arr;
            x1.IsEqual(1);
            x2.IsEqual(2);
            rest.IsEqual(new int[] { 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public void Deconstruct_3_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, x2, x3, rest) = arr;
            x1.IsEqual(1);
            x2.IsEqual(2);
            x3.IsEqual(3);
            rest.IsEqual(new int[] { 4, 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public void Deconstruct_4_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, x2, x3, x4, rest) = arr;
            x1.IsEqual(1);
            x2.IsEqual(2);
            x3.IsEqual(3);
            x4.IsEqual(4);
            rest.IsEqual(new int[] { 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public void Deconstruct_5_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, x2, x3, x4, x5, rest) = arr;
            x1.IsEqual(1);
            x2.IsEqual(2);
            x3.IsEqual(3);
            x4.IsEqual(4);
            x5.IsEqual(5);
            rest.IsEqual(new int[] { 6, 7, 8, 9, 10 });
        }

        [Fact]
        public void Deconstruct_6_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, x2, x3, x4, x5, x6, rest) = arr;
            x1.IsEqual(1);
            x2.IsEqual(2);
            x3.IsEqual(3);
            x4.IsEqual(4);
            x5.IsEqual(5);
            x6.IsEqual(6);
            rest.IsEqual(new int[] { 7, 8, 9, 10 });
        }

        [Fact]
        public void Deconstruct_7_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, x2, x3, x4, x5, x6, x7, rest) = arr;
            x1.IsEqual(1);
            x2.IsEqual(2);
            x3.IsEqual(3);
            x4.IsEqual(4);
            x5.IsEqual(5);
            x6.IsEqual(6);
            x7.IsEqual(7);
            rest.IsEqual(new int[] { 8, 9, 10 });
        }

        [Fact]
        public void Deconstruct_8_Works()
        {
            // arrange
            var arr = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            // act & assert

            var (x1, x2, x3, x4, x5, x6, x7, x8, rest) = arr;
            x1.IsEqual(1);
            x2.IsEqual(2);
            x3.IsEqual(3);
            x4.IsEqual(4);
            x5.IsEqual(5);
            x6.IsEqual(6);
            x7.IsEqual(7);
            x8.IsEqual(8);
            rest.IsEqual(new int[] { 9, 10 });
        }
    }
}