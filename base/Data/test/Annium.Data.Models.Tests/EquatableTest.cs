using System.Collections.Generic;
using System.Linq;
using Annium.Testing;

namespace Annium.Data.Models.Tests
{
    public class SampleTestClass
    {
        [Fact]
        public void EqualHashCode_ProvidesCorrectEqualityComparisons()
        {
            // arrange
            var hashSet = new HashSet<Point>();
            var list = new List<Point>();
            var a = new Point(1, 2);
            var b = new Point(1, 2);

            // act
            list.Add(a);
            list.Add(b);

            // assert
            (a == b).IsTrue();
            (b == a).IsTrue();
            a.Equals(b).IsTrue();
            b.Equals(a).IsTrue();
            hashSet.Add(a).IsTrue();
            hashSet.Add(b).IsFalse();
            list.Has(2);
            list.Distinct().Has(1);
        }

        [Fact]
        public void NullComparison_IsHandledCorrectly()
        {
            // arrange
            var a = new Point(1, 2);
            Point b = null!;
            Point c = null!;

            // assert
            (!(a == null!)).IsTrue();
            (a! != null!).IsTrue();
            (null! != a!).IsTrue();
            (b == c).IsTrue();
            (!a!.Equals(null!)).IsTrue();
            (!a!.Equals(null!)).IsTrue();
        }

        [Fact]
        public void DifferentHashCode_ProvidesCorrectEqualityComparisons()
        {
            // arrange
            var hashSet = new HashSet<Point>();
            var list = new List<Point>();
            var a = new Point(1, 2);
            var b = new Point(2, 1);

            // act
            list.Add(a);
            list.Add(b);

            // assert
            (a != b).IsTrue();
            (b != a).IsTrue();
            (!a.Equals(b)).IsTrue();
            (!b.Equals(a)).IsTrue();
            hashSet.Add(a).IsTrue();
            hashSet.Add(b).IsTrue();
            list.Has(2);
            list.Distinct().Has(2);
        }
    }
}