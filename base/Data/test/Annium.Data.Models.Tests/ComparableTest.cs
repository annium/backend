using System;
using System.Linq;
using Annium.Testing;
using Xunit;

namespace Annium.Data.Models.Tests
{
    public class ComparableTest
    {
        [Fact]
        public void DirectComparison_WorksCorrectly()
        {
            // arrange
            var a = new Money(1, 1);
            var b = new Money(1, 2);
            var c = new Money(2, 1);
            Action deniedObjectOther = () => a.CompareTo(10);

            // assert
            a.CompareTo((null as Money) !).IsEqual(1);
            deniedObjectOther.Throws<ArgumentException>();
            a.CompareTo(a as object).IsEqual(0);
            a.CompareTo(b as object).IsEqual(-1);
            a.CompareTo(c as object).IsEqual(-1);
            b.CompareTo(a as object).IsEqual(1);
            b.CompareTo(c as object).IsEqual(-1);
            c.CompareTo(a as object).IsEqual(1);
            c.CompareTo(b as object).IsEqual(1);
        }

        [Fact]
        public void IndirectComparison_WorksCorrectly()
        {
            // arrange
            var a = new Money(1, 1);
            var d = a;
            var b = new Money(1, 2);
            var c = new Money(2, 1);

            // assert
            new[] { a, b, c, null }.Max().IsEqual(c);
            // reference comparison
            (a > d).IsFalse();
            ((null as Money) ! <= (null as Money) !).IsTrue();
            // >
            (a > b).IsFalse();
            (null! > a).IsFalse();
            (a > null!).IsTrue();
            // <
            (a < b).IsTrue();
            (null! < a).IsTrue();
            (a < null!).IsFalse();
            // >=
            (a >= b).IsFalse();
            (null! >= a).IsFalse();
            (a >= null!).IsTrue();
            // <=
            (a <= b).IsTrue();
            (null! <= a).IsTrue();
            (a <= null!).IsFalse();
        }
    }
}